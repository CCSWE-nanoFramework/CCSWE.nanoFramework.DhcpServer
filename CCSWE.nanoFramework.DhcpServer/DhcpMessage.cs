using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace CCSWE.nanoFramework.DhcpServer
{
    /// <summary>
    /// DHCP Message class.
    /// </summary>
    [Obsolete("Switch to new Message")]
    internal class DhcpMessage
    {
        // Based on RFC2131 I think this value may be incorrect:
        //
        // The 'options' field is now variable length. A DHCP client must be
        // prepared to receive DHCP messages with an 'options' field of at least
        // length 312 octets.  This requirement implies that a DHCP client must
        // be prepared to receive a message of up to 576 octets
        // 
        // That being said since we're only using it for our response it may be ok
        private const int PacketSize = 300;

        private const int MagicCookieIndex = 236;
        private const int OptionsIndex = 240;

        public DhcpMessage(ref byte[] packet)
        {
            
            // See the build function for details on a message.
            const int LongSize = 4;
            var index = 0;
            
            Operation = (Operation)packet[0];
            HardwareType = (HardwareType)packet[1];
            HardwareAddressLength = packet[2];
            Hops = packet[3];
            index += LongSize;
            TransactionId = BitConverter.ToUInt32(packet, index);
            index += LongSize;
            SecondsElapsed = BitConverter.ToUInt16(packet, index);
            index += 2;
            Flags = BitConverter.ToUInt16(packet, index);
            index += 2;
            ClientIPAddress = new IPAddress(BitConverter.ToUInt32(packet, index));
            index += LongSize;
            YourIPAddress = new IPAddress(BitConverter.ToUInt32(packet, index));
            index += LongSize;
            ServerIPAddress = new IPAddress(BitConverter.ToUInt32(packet, index));
            index += LongSize;
            GatewayIPAddress = new IPAddress(BitConverter.ToUInt32(packet, index));
            index += LongSize;
            ClientHardwareAddress = new byte[HardwareAddressLength];
            Array.Copy(packet, index, ClientHardwareAddress, 0, HardwareAddressLength);
            
            Cookie = new byte[4];
            Array.Copy(packet, MagicCookieIndex, Cookie, 0, 4);

            // check copy options array
            index = OptionsIndex;
            int offset = index;
            if (packet[offset] != (byte) OptionCode.Pad)
            {
                while (packet[offset] != (byte) OptionCode.End)
                {
                    var optionCode = packet[offset++];
                    var optionLength = packet[offset++];
                    offset += optionLength;
                }

                Options = new byte[offset - index + 1];
                Array.Copy(packet, index, Options, 0, Options.Length);
            }
        }

        public TimeSpan LeaseTime
        {
            get
            {
                if (TryGetOption(OptionCode.LeaseTime, out var data))
                {
                    return TimeSpan.FromSeconds(BitConverter.ToInt32(data, 0));
                }

                return TimeSpan.Zero;
            }
            set
            {

            }
        }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        public Operation Operation { get; set; }

        /// <summary>
        /// Gets or sets the hardware type.
        /// </summary>
        public HardwareType HardwareType { get; set; }

        /// <summary>
        /// Gets or sets the hardware address length.
        /// </summary>
        public byte HardwareAddressLength { get; set; }

        /// <summary>
        /// Gets or sets the hops.
        /// </summary>
        public byte Hops { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        /// <remarks>
        /// Transaction ID, a random number chosen by the client, used by the client and server to associate
        /// messages and responses between a client and a server.
        /// </remarks>
        public uint TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the seconds elapsed.
        /// </summary>
        /// <remarks>
        /// Filled in by client, seconds elapsed since client began address acquisition or renewal process.
        /// </remarks>
        public ushort SecondsElapsed { get; set; }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        public ushort Flags { get; set; }

        /// <summary>
        /// Gets or sets the client IP address.
        /// </summary>
        /// <remarks>
        /// Client IP address; only filled in if client is in BOUND, RENEW or REBINDING state and can respond
        /// to ARP requests.
        /// </remarks>
        public IPAddress ClientIPAddress { get; set; } = new(0);

        /// <summary>
        /// Gets or sets 'your' (client) IP address.
        /// </summary>
        public IPAddress YourIPAddress { get; set; } = new(0);

        /// <summary>
        /// Gets or sets the server IP address.
        /// </summary>
        public IPAddress ServerIPAddress { get; set; } = new(0);

        /// <summary>
        /// Gets or sets the gateway IP address.
        /// </summary>
        public IPAddress GatewayIPAddress { get; set; } = new(0);

        /// <summary>
        /// Gets or sets the client hardware address.
        /// </summary>
        public byte[] ClientHardwareAddress { get; set; } = { }; // TODO: Is this correct default or should it be null?

        /// <summary>
        /// Gets or sets the magic cookie.
        /// </summary>
        public byte[] Cookie { get; set; } = { }; // TODO: Is this correct default or should it be null?

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public byte[] Options { get; set; } = { }; // TODO: Is this correct default or should it be null?

        /// <summary>
        /// Gets the message type.
        /// </summary>
        public MessageType MessageType
        {
            get
            {
                if (TryGetOption(OptionCode.DhcpMessageType, out var data))
                {
                    return (MessageType)data[0];
                }

                return MessageType.NotSet;
            }
        }

        /// <summary>
        /// Gets the host name.
        /// </summary>
        public string HostName
        {
            get
            {
                if (TryGetOption(OptionCode.HostName, out var data))
                {
                    return Encoding.UTF8.GetString(data, 0, data.Length);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the requested IP address. <see cref="OptionCode.RequestedIPAddress"/>
        /// </summary>
        public IPAddress RequestedIpAddress
        {
            get
            {
                if (TryGetOption(OptionCode.RequestedIPAddress, out var data))
                {
                    return new IPAddress(data);
                }

                return IPAddress.Any;
            }
        }

        /// <summary>
        /// The IP address of the selected server. <see cref="OptionCode.ServerIdentifier"/>
        /// </summary>
        public IPAddress ServerIdentifier
        {
            get
            {
                if (TryGetOption(OptionCode.ServerIdentifier, out var data))
                {
                    return new IPAddress(data);
                }

                return IPAddress.Any;
            }
        }

        /// <summary>
        /// Parses the message.
        /// </summary>
        /// <param name="packet">The byte array message.</param>
        private void Parse(ref byte[] packet)
        {
            // See the build function for details on a message.
            const int LongSize = 4;
            int inc = 0;
            Operation = (Operation)packet[0];
            HardwareType = (HardwareType)packet[1];
            HardwareAddressLength = packet[2];
            Hops = packet[3];
            inc += LongSize;
            TransactionId = BitConverter.ToUInt32(packet, inc);
            inc += LongSize;
            SecondsElapsed = BitConverter.ToUInt16(packet, inc);
            inc += 2;
            Flags = BitConverter.ToUInt16(packet, inc);
            inc += 2;
            ClientIPAddress = new IPAddress(BitConverter.GetBytes(BitConverter.ToUInt32(packet, inc)));
            inc += LongSize;
            YourIPAddress = new IPAddress(BitConverter.GetBytes(BitConverter.ToUInt32(packet, inc)));
            inc += LongSize;
            ServerIPAddress = new IPAddress(BitConverter.GetBytes(BitConverter.ToUInt32(packet, inc)));
            inc += LongSize;
            GatewayIPAddress = new IPAddress(BitConverter.GetBytes(BitConverter.ToUInt32(packet, inc)));
            inc += LongSize;
            ClientHardwareAddress = new byte[HardwareAddressLength];
            Array.Copy(packet, inc, ClientHardwareAddress, 0, HardwareAddressLength);
            Cookie = new byte[4];

            Array.Copy(packet, MagicCookieIndex, Cookie, 0, 4);

            // check copy options array
            inc = OptionsIndex;
            int offset = inc;
            if (packet[offset] != 0)
            {
                while (packet[offset] != (byte) OptionCode.End)
                {
                    var optionCode = packet[offset++];
                    var optionLength = packet[offset++];
                    offset += optionLength;
                }

                Options = new byte[offset - inc + 1];
                Array.Copy(packet, inc, Options, 0, Options.Length);
            }
        }

        /// <summary>
        /// Build a message.
        /// </summary>
        /// <returns>The message as a byte array.</returns>
        public byte[] Build()
        {
            // Example of a discovery message
            // byte 0  byte 1  byte 2  byte 3
            // OP      HTYPE   HLEN    HOPS
            // 0x01    0x01    0x06    0x00
            // XID
            // 0x3903F326
            // SECS            FLAGS
            // 0x0000          0x0000
            // CIADDR(Client IP address)
            // 0x00000000
            // YIADDR(Your IP address)
            // 0x00000000
            // SIADDR(Server IP address)
            // 0x00000000
            // GIADDR(Gateway IP address)
            // 0x00000000
            // CHADDR(Client hardware address)
            // 0x00053C04
            // 0x8D590000
            // 0x00000000
            // 0x00000000
            // 192 octets of 0s, or overflow space for additional options; BOOTP legacy.
            // Magic cookie
            // 0x63825363
            // DHCP options
            // 0x350101 53: 1(DHCP Discover)
            // 0x3204c0a80164 50: 192.168.1.100 requested
            // 0x370401030f06 55(Parameter Request List):
            // - 1 (Request Subnet Mask),
            // - 3 (Router),
            // - 15 (Domain Name),
            // - 6 (Domain Name Server)
            // 0xff 255(Endmark)
            const int LongSize = 4;
            int inc = 0;
            byte[] dhcpPacket = new byte[PacketSize];
            dhcpPacket[0] = (byte)Operation;
            dhcpPacket[1] = (byte)HardwareType;
            dhcpPacket[2] = HardwareAddressLength;
            dhcpPacket[3] = Hops;
            inc += LongSize;
            BitConverter.GetBytes(TransactionId).CopyTo(dhcpPacket, inc);
            inc += LongSize;
            BitConverter.GetBytes(SecondsElapsed).CopyTo(dhcpPacket, inc);

            // Only 2 bytes for the previous one
            inc += 2;
            BitConverter.GetBytes(Flags).CopyTo(dhcpPacket, inc);
            inc += 2;
            ClientIPAddress.GetAddressBytes().CopyTo(dhcpPacket, inc);
            inc += LongSize;
            YourIPAddress.GetAddressBytes().CopyTo(dhcpPacket, inc);
            inc += LongSize;
            ServerIPAddress.GetAddressBytes().CopyTo(dhcpPacket, inc);
            inc += LongSize;
            GatewayIPAddress.GetAddressBytes().CopyTo(dhcpPacket, inc);
            inc += LongSize;
            ClientHardwareAddress.CopyTo(dhcpPacket, inc);

            // We directly jump to the Magic cookie
            inc = MagicCookieIndex;
            Cookie.CopyTo(dhcpPacket, inc);
            inc += LongSize;
            Options.CopyTo(dhcpPacket, inc);
            return dhcpPacket;
        }

        // TODO: These are kind of awful. Create a DhcpMessageBuilder. This message should be immutable
        private byte[] BuildType(MessageType messageType, IPAddress yourIpAddress, IPAddress subnetMask, IPAddress serverIdentifier, byte[]? additionalOptions = null)
        {
            Operation = Operation.BootReply;
            YourIPAddress = yourIpAddress;
            ResetOptions();
            AddOption(OptionCode.DhcpMessageType, new[] { (byte)messageType });
            if (messageType != MessageType.Nak)
            {
                AddOption(OptionCode.SubnetMask, subnetMask.GetAddressBytes());
                AddOption(OptionCode.ServerIdentifier, serverIdentifier.GetAddressBytes());
            }

            if (additionalOptions != null)
            {
                AddOptions(ref additionalOptions);
            }

            return Build();
        }

        /// <summary>
        /// Offer message.
        /// </summary>
        /// <param name="cip">Client IP address..</param>
        /// <param name="mask">Network subnetMask.</param>
        /// <param name="sip">Server IP address.</param>
        /// <param name="additionalOptions">Additional options to send.</param>
        /// <returns>A byte array with the message.</returns>
        public byte[] Offer(IPAddress cip, IPAddress mask, IPAddress sip, byte[]? additionalOptions = null) => BuildType(MessageType.Offer, cip, mask, sip, additionalOptions);

        /// <summary>
        /// Acknowledge message.
        /// </summary>
        /// <param name="cip">Client IP address..</param>
        /// <param name="mask">Network subnetMask.</param>
        /// <param name="sip">Server IP address.</param>
        /// <param name="additionalOptions">Additional options to send.</param>
        /// <returns>A byte array with the message.</returns>
        public byte[] Acknowledge(IPAddress cip, IPAddress mask, IPAddress sip, byte[]? additionalOptions = null) => BuildType(MessageType.Ack, cip, mask, sip, additionalOptions);

        /// <summary>
        /// Not Acknowledge message.
        /// </summary>
        /// <returns>A byte array with the message.</returns>
        public byte[] NotAcknowledge()
        {
            // TODO: This should be setting the correct Server Identifier (check the other fields)
            YourIPAddress = IPAddress.Any;
            return BuildType(MessageType.Nak, IPAddress.Any, IPAddress.Any, IPAddress.Any);
        }

        /// <summary>
        /// Decline message.
        /// </summary>
        /// <param name="cip">Client IP address..</param>
        /// <param name="mask">Network subnetMask.</param>
        /// <param name="sip">Server IP address.</param>
        /// <returns>A byte array with the message.</returns>
        public byte[] Decline(IPAddress cip, IPAddress mask, IPAddress sip) => BuildType(MessageType.Decline, cip, mask, sip);

        private int IndexOfOption(OptionCode option)
        {
            if (Options.Length == 0)
            {
                return -1;
            }

            var index = 0;

            if (Options[index] != (byte)OptionCode.Pad)
            {
                while (Options[index] != (byte)OptionCode.End)
                {
                    var optionCode = Options[index++];
                    int optionLength = Options[index++];

                    if ((OptionCode) optionCode == option)
                    {
                        return index - 2;
                    }

                    index += optionLength;
                }
            }

            return -1;
        }

        /// <summary>
        /// Resets the options.
        /// </summary>
        public void ResetOptions()
        {
            // 240 is where the options are starting, right after the magic cookie
            Options = new byte[PacketSize - OptionsIndex];
            Options[0] = (byte) OptionCode.End;
        }

        /// <summary>
        /// Add an option to the options.
        /// </summary>
        /// <param name="option">The option code.</param>
        /// <param name="data">The option data.</param>
        public void AddOption(OptionCode option, byte[] data)
        {
            var optionData = new byte[2 + data.Length];
            optionData[0] = (byte)option;
            optionData[1] = (byte)data.Length;
            data.CopyTo(optionData, 2);
            AddOptions(ref optionData);
        }

        /// <summary>
        /// Adds options to the end of the <see cref="Options"/>, you are responsible to use the proper code and encoding.
        /// </summary>
        /// <param name="data">The options to add.</param>
        private void AddOptions(ref byte[] data)
        {
            var offset = 0;
            while (Options[offset] != (byte)OptionCode.End)
            {
                var optionCode = Options[offset++];
                var optionLength = Options[offset++];
                offset += optionLength;
            }

            data.CopyTo(Options, offset);
            Options[offset + data.Length] = (byte)OptionCode.End; // set end of options
        }

        /// <summary>
        /// Checks if the option contains a specific key.
        /// </summary>
        /// <param name="option">The option to check.</param>
        /// <returns>True if found.</returns>
        public bool HasOption(OptionCode option) => IndexOfOption(option) != -1;

        /// <summary>
        /// Gets the option contained in a key.
        /// </summary>
        /// <param name="option">The option to check.</param>
        /// <returns>The byte array with the raw option value.</returns>
        public byte[]? GetOption(OptionCode option)
        {
            var optionIndex = IndexOfOption(option);
            if (optionIndex == -1)
            {
                return null;
            }

            var optionData = new byte[Options[optionIndex + 1]];
            Array.Copy(Options, optionIndex + 2, optionData, 0, optionData.Length);

            return optionData;
        }

        public bool TryGetOption(OptionCode option, [NotNullWhen(true)] out byte[]? data)
        {
            data = GetOption(option);

            return data is not null;
        }
    }
}
