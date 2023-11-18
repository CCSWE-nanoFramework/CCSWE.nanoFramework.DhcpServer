// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CCSWE.nanoFramework.DhcpServer
{
    /// <summary>
    /// DHCP Operation.
    /// </summary>
    public enum Operation : byte
    {
        /// <summary>
        /// Boot request.
        /// </summary>
        BootRequest = 0x01,

        /// <summary>
        /// Boot reply.
        /// </summary>
        BootReply
    }
}
