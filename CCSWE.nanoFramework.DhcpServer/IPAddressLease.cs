using System;
using System.Net;

namespace CCSWE.nanoFramework.DhcpServer
{
    // TODO: This could be DhcpClient and offered states could be handled here as well as leased
    internal class IPAddressLease
    {
        /// <summary>
        /// Lazily initialized hash code
        /// </summary>
        private int _hashCode;

        public IPAddressLease(IPAddress clientAddress, string hardwareAddress, DateTime expires) : this(clientAddress, hardwareAddress, expires, TimeSpan.MaxValue)
        {

        }

        public IPAddressLease(IPAddress clientAddress, string hardwareAddress, TimeSpan leaseTime): this(clientAddress, hardwareAddress, DateTime.UtcNow + leaseTime, leaseTime)
        {

        }

        private IPAddressLease(IPAddress clientAddress, string hardwareAddress, DateTime expires, TimeSpan leaseTime)
        {
            ClientAddress = clientAddress;
            Expires = expires;
            HardwareAddress = hardwareAddress;
            LeaseTime = leaseTime;
        }

        public IPAddress ClientAddress { get; }

        public DateTime Expires { get; private set; }

        public string HardwareAddress { get; }

        public TimeSpan LeaseTime { get; }

        public override bool Equals(object? obj)
        {
            return obj is IPAddressLease lease && Equals(lease);
        }

        public bool Equals(IPAddressLease other)
        {
            return ClientAddress.Equals(other.ClientAddress) && HardwareAddress.Equals(other.HardwareAddress);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            if (_hashCode == 0)
            {
                _hashCode = ClientAddress.GetHashCode() ^ HardwareAddress.GetHashCode();
            }

            return _hashCode;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public bool IsExpired() => DateTime.UtcNow > Expires;

        public void Renew() => Expires = DateTime.UtcNow + LeaseTime;
    }
}
