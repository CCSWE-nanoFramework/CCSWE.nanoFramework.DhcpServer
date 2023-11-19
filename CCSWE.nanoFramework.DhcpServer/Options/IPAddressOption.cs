using System.Net;

namespace CCSWE.nanoFramework.DhcpServer.Options
{
    internal class IPAddressOption: OptionBase
    {
        private IPAddress? _value;

        public IPAddressOption(byte code, byte[] data): base(code, data)
        {
            Ensure.IsValid(nameof(data), data.Length == 4);
        }

        public IPAddressOption(OptionCode code, byte[] data): this((byte)code, data) { }

        public IPAddressOption(OptionCode code, IPAddress value) : this(code, Converter.GetBytes(value))
        {
            _value = value;
        }

        public IPAddress Deserialize()
        {
            return _value ??= Converter.GetIPAddress(Data);
        }

        public static bool IsKnownOption(byte code) => IsKnownOption((OptionCode)code);

        public static bool IsKnownOption(OptionCode code)
        {
            return code switch
            {
                OptionCode.RequestedIPAddress => true,
                OptionCode.ServerIdentifier => true,
                _ => false
            };
        }
    }
}
