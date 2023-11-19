namespace CCSWE.nanoFramework.DhcpServer.Options
{
    // TODO: Not doing anything for null terminated strings at this point
    internal class StringOption: OptionBase
    {
        private string? _value;

        public StringOption(byte code, byte[] data): base(code, data) { }

        public StringOption(OptionCode code, byte[] data) : this((byte)code, data) { }

        public StringOption(OptionCode code, string value) : this(code, Converter.GetBytes(value))
        {
            _value = value;
        }

        public string Deserialize()
        {
            return _value ??= Converter.GetString(Data);
        }

        public static bool IsKnownOption(byte code) => IsKnownOption((OptionCode)code);

        public static bool IsKnownOption(OptionCode code)
        {
            return code switch
            {
                OptionCode.HostName => true,
                _ => false
            };
        }
    }
}
