using System;

namespace CCSWE.nanoFramework.DhcpServer.Options
{
    internal abstract class OptionBase: IOption
    {
        protected OptionBase(byte code, byte[] data): this(code, data, (byte)data.Length) { }

        protected OptionBase(byte code, byte[] data, byte length)
        {
            Code = code;
            Data = data;
            Length = length;
        }

        protected OptionBase(OptionCode code, byte[] data) : this((byte)code, data) { }

        public byte Code { get; }
        public byte[] Data { get; }
        public byte Length { get; }

        public ushort OptionLength => (ushort)(Length + 2);

        /// <summary>
        /// Converts the <see cref="IOption"/> to a <see cref="T:byte[]"/>.
        /// </summary>
        public byte[] GetBytes()
        {
            var data = new byte[OptionLength];
            data[0] = Code;
            data[1] = Length;

            Array.Copy(Data, 0, data, 2, Length);

            return data;
        }
    }
}
