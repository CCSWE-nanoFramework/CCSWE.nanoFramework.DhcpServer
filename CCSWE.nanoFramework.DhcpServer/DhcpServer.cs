// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CCSWE.nanoFramework.DhcpServer
{
    /// <summary>
    /// A simple DHCP server.
    /// </summary>
    public sealed class DhcpServer : IDisposable
    {
        // TODO: Replace with IPAddress.Broadcast when it's merged in
        private static readonly IPAddress Broadcast = new(new byte[] { 255, 255, 255, 255 });

        private const int ServerPort = 67;
        private const int ClientPort = 68;

        private readonly ILogger? _logger;

        private IPAddressPool? _addressPool;
        private Socket? _requestSocket;
        private Socket? _responseSocket;

        private Thread _dhcpServerThread;
        private bool _islistening;
        private IPAddress _serverAddress;
        private IPAddress _mask;
        private Timer _timer;
        private ushort _timeToLeave;

        public DhcpServer(ILogger? logger = null)
        {
            _logger = logger;
        }

        // TODO: This should be moved to Start
        /// <summary>
        /// Gets or sets the captive portal URL. If null or empty, this will be ignored.
        /// </summary>
        public string CaptivePortalUrl { get; set; }

        private string FormatLogMessage(string message) => $"[{nameof(DhcpServer)}] {message}";

        private void HandleDiscoverMessage(DhcpMessage message)
        {
            if (!_addressPool.IsAddressAvailable())
            {
                Log(LogLevel.Trace, "No more addresses available.");
                return;
            }

            var hardwareAddress = BitConverter.ToString(message.ClientHardwareAddress);
            IPAddress? yourIp = null;

            // Do we have an option asking for a specific IP address?
            var requestedIpAddress = message.RequestedIpAddress;
            if (!requestedIpAddress.Equals(IPAddress.Any) && _addressPool.IsLeasedTo(requestedIpAddress, hardwareAddress))
            {
                yourIp = requestedIpAddress;
            }

            yourIp ??= _addressPool.Get();

            if (yourIp is not null)
            {
                var offer = message.Offer(yourIp, _mask, _serverAddress, GetAdditionalOptions());
                _responseSocket.Send(offer);
            }
            else
            {
                Log(LogLevel.Trace, "No more addresses available.");
            }
        }

        private void HandleRequestMessage(DhcpMessage message)
        {
            // TODO: Handle this case -> https://github.com/jpmikkers/DHCPServer/blob/d224f1c37dcb9b4352cd3741be46a8e3eee5dcb9/DHCPServer/Library/DHCPServer.cs#L914
            // no server identifier: the message is a request to verify or extend an existing lease
            // Received REQUEST without server identifier, client is INIT-REBOOT, RENEWING or REBINDING

            // Check the request is for us
            var serverIdentifier = message.ServerIdentifier;
            if (!serverIdentifier.Equals(_serverAddress) && !serverIdentifier.Equals(IPAddress.Any))
            {
                return;
            }

            var hardwareAddress = BitConverter.ToString(message.ClientHardwareAddress);

            var requestSucceeded = _addressPool.IsLeased(message.RequestedIpAddress)
                ? _addressPool.Renew(message.RequestedIpAddress, hardwareAddress)
                : _addressPool.Request(message.RequestedIpAddress, hardwareAddress, TimeSpan.FromMinutes(30));

            _responseSocket.Send(requestSucceeded
                ? message.Acknowledge(message.RequestedIpAddress, _mask, _serverAddress, GetAdditionalOptions())
                : message.NotAcknowledge());
        }

        private void Log(LogLevel logLevel, string message, Exception? exception = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (_logger is null)
            {
#if !DEBUG
                if (logLevel > LogLevel.Trace)
                {
#endif
                    Debug.WriteLine(FormatLogMessage(message));
#if !DEBUG
                }
#endif
                return;
            }

            _logger.Log(logLevel, exception, FormatLogMessage(message));
        }

        private void LogMessage(DhcpMessage message)
        {
#if !DEBUG
            return;
#endif

            var messageType = message.MessageType.AsString();
            var transactionId = message.TransactionId.ToString("X");

            if (!string.IsNullOrEmpty(message.HostName))
            {
                Log(LogLevel.Trace, $"[{messageType}] ({transactionId}) Host name: {message.HostName}");
            }

            Log(LogLevel.Trace, $"[{messageType}] ({transactionId}) Hardware address: {BitConverter.ToString(message.ClientHardwareAddress)}");

            if (message.LeaseTime > TimeSpan.Zero)
            {
                Log(LogLevel.Trace, $"[{messageType}] ({transactionId}) Lease time: {message.LeaseTime}");
            }

            if (!message.RequestedIpAddress.Equals(IPAddress.Any))
            {
                Log(LogLevel.Trace, $"[{messageType}] ({transactionId}) Requested address: {message.RequestedIpAddress}");
            }

            if (!message.ServerIdentifier.Equals(IPAddress.Any))
            {
                Log(LogLevel.Trace, $"[{messageType}] ({transactionId}) Server identifier: {message.ServerIdentifier}");
            }
        }

        /// <summary>
        /// Starts the DHCP Server to start listening.
        /// </summary>
        /// <returns>Returns false in case of error.</returns>
        /// <param name="serverAddress">The server IP address.</param>
        /// <param name="mask">The mask used for distributing the IP address.</param>
        /// <param name="timeToLeave">Default time to leave for bail expiration.</param>
        /// <exception cref="SocketException">Socket exception occurred.</exception>
        /// <exception cref="Exception">An exception occurred while setting up the DHCP listener or sender.</exception>
        public bool Start(IPAddress serverAddress, IPAddress mask, ushort timeToLeave = 1200)
        {
            if (_requestSocket is null)
            {
                try
                {
                    _addressPool = new IPAddressPool(serverAddress);
                    _serverAddress = serverAddress;
                    _mask = mask;

                    _requestSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _requestSocket.Bind(new IPEndPoint(Broadcast, ServerPort));

                    _responseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _responseSocket.Bind(new IPEndPoint(_serverAddress, 0));
                    _responseSocket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.Broadcast, true);
                    _responseSocket.Connect(new IPEndPoint(Broadcast, ClientPort));

                    _timeToLeave = timeToLeave;

                    // TODO: Need to clean this up when stopped
                    // TODO: Period should be one minute?
                    _timer = new Timer(_ => { _addressPool.Evict(); }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

                    // start server thread
                    _dhcpServerThread = new Thread(RunServer);
                    _dhcpServerThread.Start();

                    return true;
                }
                catch (SocketException ex)
                {
                    Log(LogLevel.Error, $"Socket exception occurred. Code: {ex.ErrorCode} Message: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"Exception occurred. Message: {ex.Message}", ex);
                }

                return false;
            }

            // It's already started
            return true;
        }

        /// <summary>
        /// Stops the listening.
        /// </summary>
        public void Stop()
        {
            _islistening = false;
        }

        private void RunServer()
        {
            _islistening = true;

            // setup buffer to read data from socket
            var buffer = new byte[1024];

            while (_islistening)
            {
                try
                {
                    var bytes = _requestSocket.Receive(buffer);

                    if (bytes > 0)
                    {
                        var message = new DhcpMessage(ref buffer);

                        // Only response to requests
                        if (message.Operation != Operation.BootRequest)
                        {
                            continue;
                        }

                        LogMessage(message);

                        // TODO: Handle Release and Renew
                        // TODO: Handle lease time
                        switch (message.MessageType)
                        {
                            case MessageType.Discover:
                                HandleDiscoverMessage(message);
                                break;

                            case MessageType.Request:
                                HandleRequestMessage(message);
                                break;

                            default:
                                Log(LogLevel.Trace, $"Unhandled message type '{message.MessageType}' received from host: {message.HostName}");
                                break;
                        }
                    }
                    else
                    {
                        // free cpu time if no bytes in socket
                        Thread.Sleep(200);
                    }
                }
                catch
                {
                    //// Just pass this, we want to make sure that this loop always works properly.
                }
            }

            try
            {
                _requestSocket.Close();
                _responseSocket.Close();
            }
            catch
            {
                //// Make sure we catch everything coming in
            }

            _requestSocket = null;
            _responseSocket = null;

            Log(LogLevel.Information, "Stopped");
        }

        private byte[] GetAdditionalOptions()
        {
            byte[] additionalOptions = null;

            if (!string.IsNullOrEmpty(CaptivePortalUrl))
            {
                var encoded = Encoding.UTF8.GetBytes(CaptivePortalUrl);
                additionalOptions = new byte[2 + encoded.Length];
                additionalOptions[0] = (byte)OptionCode.CaptivePortal;
                additionalOptions[1] = (byte)CaptivePortalUrl.Length;
                encoded.CopyTo(additionalOptions, 2);
            }

            return additionalOptions;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
        }
    }
}
