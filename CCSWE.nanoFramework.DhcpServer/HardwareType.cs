using System;

namespace CCSWE.nanoFramework.DhcpServer
{
    /// <summary>
    /// Hardware type.
    /// </summary>
    /// <remarks>
    /// IANA list https://www.iana.org/assignments/arp-parameters/arp-parameters.xhtml
    /// </remarks>
    [Obsolete("Knowing these values provides no benefit for our use case")]
    public enum HardwareType
    {
        /// <summary>Ethernet.</summary>
        Ethernet = 0x01,

        /// <summary>Experimental ethernet.</summary>
        ExperimentalEthernet,

        /// <summary>Amateur radio.</summary>
        AmateurRadio,

        /// <summary>Proteon token ring.</summary>
        ProteonTokenRing,

        /// <summary>Chaos.</summary>
        Chaos,

        /// <summary>IEEE802 networks.</summary>
        IEEE802Networks,

        /// <summary>ARC Net.</summary>
        ArcNet,

        /// <summary>Hyper channel.</summary>
        HyperChannel,

        /// <summary>Lanstar.</summary>
        Lanstar
    }
}
