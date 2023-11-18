// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace CCSWE.nanoFramework.DhcpServer
{
    /// <summary>
    /// The type of DHCP Messages.
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>Unknown.</summary>
        Unknown = 0x00,

        /// <summary>Discover.</summary>
        Discover,

        /// <summary>Offer.</summary>
        Offer,

        /// <summary>Request.</summary>
        Request,

        /// <summary>Decline.</summary>
        Decline,

        /// <summary>Acknowledged.</summary>
        Ack,

        /// <summary>Not acknowledged.</summary>
        Nak,

        /// <summary>Release.</summary>
        Release,

        /// <summary>Inform.</summary>
        Inform,

        /// <summary>Force renew.</summary>
        ForceRenew,

        /// <summary>Lease query.</summary>
        LeaseQuery,

        /// <summary>Lease unassigned.</summary>
        LeaseUnassigned,

        /// <summary>Lease unknown.</summary>
        LeaseUnknown,

        /// <summary>Lease active.</summary>
        LeaseActive
    }

    internal static class MessageTypeExtensions
    {
        public static string AsString(this MessageType type)
        {
            return type switch
            {
                MessageType.Unknown => "UNKNOWN",
                MessageType.Discover => "DISCOVER",
                MessageType.Offer => "OFFER",
                MessageType.Request => "REQUEST",
                MessageType.Decline => "DECLINE",
                MessageType.Ack => "ACK",
                MessageType.Nak => "NAK",
                MessageType.Release => "RELEASE",
                MessageType.Inform => "INFORM",
                MessageType.ForceRenew => "FORCE RENEW",
                MessageType.LeaseQuery => "LEASE QUERY",
                MessageType.LeaseUnassigned => "LEASE UNASSIGNED",
                MessageType.LeaseUnknown => "LEASE UNKNOWN",
                MessageType.LeaseActive => "LEASE ACTIVE",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
