using System;

namespace CCSWE.nanoFramework.DhcpServer.Options
{
    // TODO: TimeSpan only supports seconds. Is this an issue?
    internal class TimeSpanOption: OptionBase
    {
        private bool _deserialized;
        private TimeSpan _value;

        public TimeSpanOption(byte code, byte[] data): base(code, data) { }

        public TimeSpanOption(OptionCode code, byte[] data) : this((byte)code, data) { }

        public TimeSpanOption(OptionCode code, TimeSpan value) : this(code, Converter.GetBytes(value))
        {
            _deserialized = true;
            _value = value;
        }

        public TimeSpan Deserialize()
        {
            if (!_deserialized)
            {
                _deserialized = true;
                _value = Converter.GetTimeSpan(Data);
            }

            return _value;
        }

        public static bool IsKnownOption(byte code) => IsKnownOption((OptionCode)code);

        public static bool IsKnownOption(OptionCode code)
        {
            return code switch
            {
                OptionCode.LeaseTime => true,
                _ => false
            };
        }
    }
}
