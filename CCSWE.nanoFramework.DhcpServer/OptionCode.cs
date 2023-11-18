namespace CCSWE.nanoFramework.DhcpServer
{
    /// <summary>
    /// DHCP option code. See https://www.ibm.com/docs/en/i/7.2?topic=concepts-dhcp-options-lookup for a comprehensive definition of each of them.
    /// Also present in https://en.wikipedia.org/wiki/Dynamic_Host_Configuration_Protocol.
    /// Authoritative source? https://www.rfc-editor.org/rfc/rfc2132.txt
    /// This enumeration is not complete and more options are available.
    /// </summary>
    public enum OptionCode : byte
    {
        /// <summary>
        /// The pad option can be used to cause subsequent fields to align on
        /// word boundaries.
        /// 
        /// The code for the pad option is 0, and its length is 1 octet.
        /// </summary>
        Pad = 0,

        /// <summary>
        /// The subnet mask option specifies the client's subnet mask as per RFC
        /// 950 [5].
        /// 
        /// If both the subnet mask and the router option are specified in a DHCP
        /// reply, the subnet mask option MUST be first.
        ///
        /// The code for the subnet mask option is 1, and its length is 4 octets.
        /// </summary>
        SubnetMask = 1,

        /// <summary>
        /// The time offset field specifies the offset of the client's subnet in
        /// seconds from Coordinated Universal Time (UTC).  The offset is
        /// expressed as a two's complement 32-bit integer.  A positive offset
        /// indicates a location east of the zero meridian and a negative offset
        /// indicates a location west of the zero meridian.
        /// 
        /// The code for the time offset option is 2, and its length is 4 octets.
        /// </summary>
        TimeOffset = 2,

        /// <summary>
        /// The router option specifies a list of IP addresses for routers on the
        /// client's subnet.  Routers SHOULD be listed in order of preference.
        /// 
        /// The code for the router option is 3.  The minimum length for the
        /// router option is 4 octets, and the length MUST always be a multiple
        /// of 4.
        /// </summary>
        Router = 3,

        /// <summary>
        /// The time server option specifies a list of RFC 868 [6] time servers
        /// available to the client.  Servers SHOULD be listed in order of
        /// preference.
        /// 
        /// The code for the time server option is 4.  The minimum length for
        /// this option is 4 octets, and the length MUST always be a multiple of
        /// 4.
        /// </summary>
        TimeServer = 4,

        /// <summary>Name server.</summary>
        NameServer = 5,

        /// <summary>Domain name server.</summary>
        DomainNameServer = 6,

        /// <summary>Log server.</summary>
        LogServer = 7,

        /// <summary>Cookie server.</summary>
        CookieServer = 8,

        /// <summary>Line printer server.</summary>
        LinePrinterServer = 9,

        /// <summary>Impress server.</summary>
        ImpressServer = 10,

        /// <summary>Resource location server.</summary>
        ResourceLocationServer = 11,

        /// <summary>
        /// This option specifies the name of the client.  The name may or may
        /// not be qualified with the local domain name (see section 3.17 for the
        /// preferred way to retrieve the domain name).  See RFC 1035 for
        /// character set restrictions.
        /// 
        /// The code for this option is 12, and its minimum length is 1.
        /// </summary>
        HostName = 12,

        /// <summary>Boot file size.</summary>
        BootFileSize = 13,

        /// <summary>Merit dump file.</summary>
        MeritDumpFile = 14,

        /// <summary>Domain name suffix.</summary>
        DomainNameSuffix = 15,

        /// <summary>Swap server.</summary>
        WrapServer = 16,

        /// <summary>Root path.</summary>
        RootPath = 17,

        /// <summary>Extensions path.</summary>
        ExtensionsPath = 18,

        /// <summary>IP forwarding.</summary>
        IpForwarding = 19,

        /// <summary>Non-Local source routing.</summary>
        NonLocalSourceRouting = 20,

        /// <summary>Policy filter.</summary>
        PolicyFilter = 21,

        /// <summary>Maximum datagram reassembly size.</summary>
        MaximumDatagramReassemblySize = 22,

        /// <summary>Default IP time to live.</summary>
        DefaultIpTimeToLive = 23,

        /// <summary>Path MTU aging timeout.</summary>
        PathMtuAgingTimeout = 24,

        /// <summary>Path MTU plateau table.</summary>
        PathMtuPlateauTable = 25,

        /// <summary>Interface MTU.</summary>
        InterfaceMtu = 26,

        /// <summary>All subnets are local.</summary>
        AllSubnetsAreLocal = 27,

        /// <summary>Broadcast address.</summary>
        BroadcastAddress = 28,

        /// <summary>Perform mask discovery.</summary>
        PerformMaskDiscovery = 29,

        /// <summary>Mask supplier.</summary>
        MaskSupplier = 30,

        /// <summary>Perform router discovery.</summary>
        PerformRouterDiscovery = 31,

        /// <summary>Router solicitation address option.</summary>
        RouterSolicitationAddressOption = 32,

        /// <summary>Static route.</summary>
        StaticRoute = 33,

        /// <summary>Trailer encapsulation.</summary>
        TrailerEncapsulation = 34,

        /// <summary>ARP cache timeout.</summary>
        ArpCacheTimeout = 35,

        /// <summary>Ethernet encapsulation.</summary>
        EthernetEncapsulation = 36,

        /// <summary>TCP default TTL.</summary>
        TcpDefaultTtl = 37,

        /// <summary>TCP keep-alive interval.</summary>
        TcpKeepAliveInterval = 38,

        /// <summary>TCP keep-alive garbage.</summary>
        TcpKeepAliveGarbage = 39,

        /// <summary>Network information service domain.</summary>
        NetworkInformationServiceDomain = 40,

        /// <summary>Network information servers.</summary>
        NetworkInformationServers = 41,

        /// <summary>Network time protocol servers option.</summary>
        NetworkTimeProtocolServersOption = 42,

        /// <summary>NetBIOS over TCP/IP name server.</summary>
        NetBiosTcpNameServer = 44,

        /// <summary>NetBIOS over TCP/IP datagram distribution server.</summary>
        NetBiosTcpDatagramDistributionServer = 45,

        /// <summary>NetBIOS over TCP/IP node type.</summary>
        NetBiosTcpNodeType = 46,

        /// <summary>NetBIOS over TCP/IP scope.</summary>
        NetBiosTcpScope = 47,

        /// <summary>X Window System Font server.</summary>
        XWindowSystemFontServer = 48,

        /// <summary>X Window System display manager.</summary>
        XWindowSystemDisplayManager = 49,

        /// <summary>
        /// This option is used in a client request (DHCPDISCOVER) to allow the
        /// client to request that a particular IP address be assigned.
        /// 
        /// The code for this option is 50, and its length is 4.
        /// </summary>
        RequestedIpAddress = 50,

        /// <summary>
        /// This option is used in a client request (DHCPDISCOVER or DHCPREQUEST)
        /// to allow the client to request a lease time for the IP address.  In a
        /// server reply (DHCPOFFER), a DHCP server uses this option to specify
        /// the lease time it is willing to offer.
        /// 
        /// The time is in units of seconds, and is specified as a 32-bit
        /// unsigned integer.
        /// 
        /// The code for this option is 51, and its length is 4.
        /// </summary>
        LeaseTime = 51,

        /// <summary>
        /// This option is used to convey the type of the DHCP message.  The code
        /// for this option is 53, and its length is 1.  Legal values for this
        /// option are defined in <see cref="MessageType"/>.
        /// </summary>
        DhcpMessageType = 53,

        /// <summary>
        /// This option is used in DHCPOFFER and DHCPREQUEST messages, and may
        /// optionally be included in the DHCPACK and DHCPNAK messages.  DHCP
        /// servers include this option in the DHCPOFFER in order to allow the
        /// client to distinguish between lease offers.  DHCP clients use the
        /// contents of the 'server identifier' field as the destination address
        /// for any DHCP messages unicast to the DHCP server.  DHCP clients also
        /// indicate which of several lease offers is being accepted by including
        /// this option in a DHCPREQUEST message.
        /// 
        /// The identifier is the IP address of the selected server.
        /// 
        /// The code for this option is 54, and its length is 4.
        /// </summary>
        ServerIdentifier = 54,

        /// <summary>Parameter list.</summary>
        ParameterList = 55,

        /// <summary>DHCP Message.</summary>
        DhcpMessage = 56,

        /// <summary>DHCP maximum message size.</summary>
        DhcpMaxMessageSize = 57,

        /// <summary>Renewal (T1) time value.</summary>
        RenewalT1 = 58,

        /// <summary>Rebinding (T2) time value.</summary>
        RebindingT2 = 59,

        /// <summary>Class ID.</summary>
        ClassId = 60,

        /// <summary>Client ID.</summary>
        ClientId = 61,

        /// <summary>NetWare/IP domain name.</summary>
        NetWareIpDomainName = 62,

        /// <summary>NetWare/IP.</summary>
        NetWareIp = 63,

        /// <summary>NIS domain name.</summary>
        NisDomainName = 64,

        /// <summary>NIS servers.</summary>
        NisServer = 65,

        /// <summary>Server name.</summary>
        ServerName = 66,

        /// <summary>Boot file name.</summary>
        BootFileName = 67,

        /// <summary>Home address.</summary>
        HomeAddress = 68,

        /// <summary>SMTP servers.</summary>
        SmtpServers = 69,

        /// <summary>POP3 server.</summary>
        Pop3Server = 70,

        /// <summary>NNTP server.</summary>
        NntpServer = 71,

        /// <summary>WWW Server.</summary>
        WwwServer = 72,

        /// <summary>Finger server.</summary>
        FingerServer = 73,

        /// <summary>IRC server.</summary>
        IrcServer = 74,

        /// <summary>StreetTalk server.</summary>
        StreetTalkServer = 75,

        /// <summary>STDA server.</summary>
        StdaServer = 76,

        /// <summary>User class.</summary>
        UserClass = 77,

        /// <summary>Directory agent.</summary>
        DirectoryAgent = 78,

        /// <summary>Service scope.</summary>
        ServiceScope = 79,

        /// <summary>Naming authority.</summary>
        NamingAuthority = 80,

        /// <summary>Auto config.</summary>
        AutoConfig = 0x74,

        /// <summary>Captive portal. See RFC8910.</summary>
        CaptivePortal = 160,

        /// <summary>
        /// The end option marks the end of valid information in the vendor
        /// field. Subsequent octets should be filled with pad options.
        /// 
        /// The code for the end option is 255, and its length is 1 octet.
        /// </summary>
        End = 255
    }
}
