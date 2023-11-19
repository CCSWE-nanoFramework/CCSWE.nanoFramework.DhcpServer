namespace CCSWE.nanoFramework.DhcpServer.Options
{
    internal interface IOption
    {
        public byte Code { get; }
        public byte[] Data { get; }
        public byte Length { get; }

        public ushort OptionLength { get; }

        public byte[] GetBytes();

        // TODO: Report bug with default properties
        //public uint OptionLength => (uint)(Length + 2);
        // TODO: And methods?
        //public uint GetSerializedLength() => (uint)(Length + 2);

        /*
        public byte[] GetBytes()
        {
            var data = new byte[GetSerializedLength()];
            data[0] = Code;
            data[1] = Length;

            Array.Copy(Data, 0, data, 2, Length);

            return data;
        }
        */
    }
}
