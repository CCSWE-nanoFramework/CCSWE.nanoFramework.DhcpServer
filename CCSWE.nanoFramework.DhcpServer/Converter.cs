using System;
using System.Net;
using System.Text;

namespace CCSWE.nanoFramework.DhcpServer
{
    internal class Converter
    {
        /// <summary>
        /// Copies <paramref name="source"/> to <paramref name="destination"/> starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="source">The source data.</param>
        /// <param name="destination">The destination array.</param>
        /// <param name="index">The start index.</param>
        /// <returns>The number of bytes copied.</returns>
        public static int CopyTo(byte source, byte[] destination, int index)
        {
            destination[index] = source;
            return 1;
        }

        /// <inheritdoc cref="CopyTo(byte,byte[],int)"/>
        public static int CopyTo(byte[] source, byte[] destination, int index)
        {
            source.CopyTo(destination, index);
            return source.Length;
        }

        /// <inheritdoc cref="CopyTo(byte,byte[],int)"/>
        public static int CopyTo(IPAddress source, byte[] destination, int index) => CopyTo(GetBytes(source), destination, index);

        /// <inheritdoc cref="CopyTo(byte,byte[],int)"/>
        public static int CopyTo(uint source, byte[] destination, int index) => CopyTo(GetBytes(source), destination, index);

        /// <inheritdoc cref="CopyTo(byte,byte[],int)"/>
        public static int CopyTo(ushort source, byte[] destination, int index) => CopyTo(GetBytes(source), destination, index);

        public static int CopyTo(byte[] source, int sourceIndex, byte[] destination, int destinationIndex = 0, int length = 0)
        {
            Array.Copy(source, sourceIndex, destination, destinationIndex, length > 0 ? length : destination.Length);
            return length;
        }

        public static byte[] GetBytes(IPAddress value) => value.GetAddressBytes();
        public static byte[] GetBytes(string value) => Encoding.UTF8.GetBytes(value);
        public static byte[] GetBytes(TimeSpan value) => BitConverter.GetBytes((int)value.TotalSeconds);
        public static byte[] GetBytes(uint value) => BitConverter.GetBytes(value);
        public static byte[] GetBytes(ushort value) => BitConverter.GetBytes(value);

        public static IPAddress GetIPAddress(byte[] data, int index = 0) => new(GetUInt32(data, index));
        public static string GetString(byte[] data, int index = 0, int length = -1) => Encoding.UTF8.GetString(data, index, length > -1 ? length : data.Length);
        public static TimeSpan GetTimeSpan(byte[] data, int index = 0) => TimeSpan.FromSeconds(GetUInt32(data, index));
        public static ushort GetUInt16(byte[] data, int index = 0) => BitConverter.ToUInt16(data, index);
        public static uint GetUInt32(byte[] data, int index = 0) => BitConverter.ToUInt32(data, index);
    }
}
