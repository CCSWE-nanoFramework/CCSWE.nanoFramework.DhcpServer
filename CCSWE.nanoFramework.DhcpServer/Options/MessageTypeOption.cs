namespace CCSWE.nanoFramework.DhcpServer.Options
{
    internal class MessageTypeOption : OptionBase
    {
        public MessageTypeOption(byte[] data): base(OptionCode.DhcpMessageType, data)
        {
            Ensure.IsValid(nameof(data), data.Length == 1);
        }

        public MessageTypeOption(MessageType messageType): this(new[] { (byte)messageType }) { }

        public MessageType Deserialize()
        {
            return (MessageType)Data[0];
        }

        public static bool IsKnownOption(byte code) => IsKnownOption((OptionCode)code);

        public static bool IsKnownOption(OptionCode code) => OptionCode.DhcpMessageType == code;
    }
}
