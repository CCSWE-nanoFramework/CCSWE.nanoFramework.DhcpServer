using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace CCSWE.nanoFramework.DhcpServer
{
    /// <summary>
    /// A class C (255.255.255.0) IP address pool
    /// </summary>
    internal class IPAddressPool
    {
        private const ushort AddressPoolSize = 254;

        private readonly Hashtable _leases = new(AddressPoolSize);
        private readonly IPAddress _serverAddress;

        public IPAddressPool(IPAddress serverAddress)
        {
            _serverAddress = serverAddress;
            _leases.Add(GetKey(serverAddress), new IPAddressLease(serverAddress, nameof(DhcpServer), DateTime.MaxValue));
        }

        public int Available => AddressPoolSize - _leases.Count;

        public void Evict()
        {
            var keyCollection = _leases.Keys;
            var keys = new byte[keyCollection.Count];

            keyCollection.CopyTo(keys, 0);

            foreach (var key in keys)
            {
                if (key <= 0)
                {
                    continue;
                }

                if (_leases[key] is IPAddressLease lease && lease.IsExpired())
                {
                    _leases.Remove(key);
                }
            }
        }

        public IPAddress? Get()
        {
            var targetAddress = _serverAddress.GetAddressBytes();

            // Always start the search at 1 for simplicity
            for (byte i = 1; i < 255; i++)
            {
                targetAddress[3] = i;

                if (!_leases.Contains(targetAddress[3]))
                {
                    return new IPAddress(targetAddress);
                }
            }

            return null;
        }

        private byte GetKey(IPAddress clientAddress)
        {
            var clientAddressBytes = clientAddress.GetAddressBytes();
            var serverAddressBytes = _serverAddress.GetAddressBytes();

            Ensure.IsInRange(nameof(clientAddress), clientAddressBytes[0] == serverAddressBytes[0] && clientAddressBytes[1] == serverAddressBytes[1] && clientAddressBytes[2] == serverAddressBytes[2]);

            return clientAddressBytes[3];
        }

        public bool IsAddressAvailable() => Available < AddressPoolSize;

        public bool IsLeased(IPAddress clientAddress) => _leases.Contains(GetKey(clientAddress));

        /// <summary>
        /// Checks if the IP address is leased to the specified hardware address.
        /// </summary>
        /// <param name="clientAddress">The IP address to check.</param>
        /// <param name="hardwareAddress">The hardware address to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="clientAddress"/> is leased to the <paramref name="hardwareAddress"/>; otherwise <see langword="false"/>.</returns>
        public bool IsLeasedTo(IPAddress clientAddress, string hardwareAddress) => IsLeasedTo(clientAddress, hardwareAddress, out _);


        /// <summary>
        /// Checks if the IP address is leased to the specified hardware address.
        /// </summary>
        /// <param name="clientAddress">The IP address to check.</param>
        /// <param name="hardwareAddress">The hardware address to check.</param>
        /// <param name="lease">The matching <see cref="IPAddressLease"/> if leased.</param>
        /// <returns><see langword="true"/> if the <paramref name="clientAddress"/> is leased to the <paramref name="hardwareAddress"/>; otherwise <see langword="false"/>.</returns>
        private bool IsLeasedTo(IPAddress clientAddress, string hardwareAddress, [NotNullWhen(true)] out IPAddressLease? lease)
        {
            lease = _leases[GetKey(clientAddress)] as IPAddressLease;

            if (lease is null)
            {
                return false;
            }

            return lease.HardwareAddress == hardwareAddress;
        }

        public void Release(IPAddress clientAddress)
        {
            _leases.Remove(GetKey(clientAddress));
        }
        
        public bool Renew(IPAddress clientAddress, string hardwareAddress)
        {
            if (!IsLeasedTo(clientAddress, hardwareAddress, out var lease))
            {
                return false;
            }

            lease.Renew();
            
            return true;
        }

        public bool Request(IPAddress clientAddress, string hardwareAddress, TimeSpan leaseTime)
        {
            var key = GetKey(clientAddress);
            if (_leases.Contains(key))
            {
                return Renew(clientAddress, hardwareAddress);
            }

            _leases[key] = new IPAddressLease(clientAddress, hardwareAddress, leaseTime);

            return true;
        }

        public bool TryGet([NotNullWhen(true)] out IPAddress? address)
        {
            address = Get();
            return address is not null;
        }
    }
}
