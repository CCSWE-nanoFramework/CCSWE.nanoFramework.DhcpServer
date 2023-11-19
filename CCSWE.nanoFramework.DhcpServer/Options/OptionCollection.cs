using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace CCSWE.nanoFramework.DhcpServer.Options
{
    /// <summary>
    /// Represents a collection of DHCP options.
    /// </summary>
    /// <remarks>
    /// The <see cref="OptionCollection"/> behaves like a dictionary and does not support multiple instances of the same <see cref="OptionCode"/>.
    /// </remarks>
    internal class OptionCollection
    {
        private int _length;
        private readonly object _lock = new();
        private readonly Hashtable _options = new();

        /// <summary>
        /// The length of the <see cref="OptionCollection"/>.
        /// </summary>
        public int Length => _length + 1;

        /// <summary>
        /// Add or replace an <see cref="IOption"/>.
        /// </summary>
        public void Add(IOption option)
        {
            lock (_lock)
            {
                if (TryGet(option.Code, out var existing))
                {
                    _length -= existing.OptionLength;
                }

                _length += option.OptionLength;
                _options[option.Code] = option;
            }
        }

        public bool Contains(byte code) => _options.Contains(code);

        public bool Contains(OptionCode code) => Contains((byte)code);

        public IOption? Get(byte code)
        {
            lock (_lock)
            {
                return Contains(code) ? (IOption)_options[code] : null;
            }
        }

        public IOption? Get(OptionCode code) => Get((byte)code);

        /// <summary>
        /// Converts the <see cref="OptionCollection"/> to a <see cref="T:byte[]"/>.
        /// </summary>
        public byte[] GetBytes()
        {
            lock (_lock)
            {
                var options = new byte[Length];
                var index = 0;

                foreach (var item in _options)
                {
                    if (item is not IOption option)
                    {
                        continue;
                    }

                    index += Converter.CopyTo(option.GetBytes(), options, index);
                }

                options[index + 1] = (byte)OptionCode.End;

                return options;
            }
        }

        public IPAddress GetOrDefault(OptionCode code, IPAddress defaultValue)
        {
            if (!TryGet(code, out var option))
            {
                return defaultValue;
            }

            return option is not IPAddressOption knownOption ? defaultValue : knownOption.Deserialize();
        }

        public string GetOrDefault(OptionCode code, string defaultValue)
        {
            if (!TryGet(code, out var option))
            {
                return defaultValue;
            }

            return option is not StringOption knownOption ? defaultValue : knownOption.Deserialize();
        }

        public TimeSpan GetOrDefault(OptionCode code, TimeSpan defaultValue)
        {
            if (!TryGet(code, out var option))
            {
                return defaultValue;
            }

            return option is not TimeSpanOption knownOption ? defaultValue : knownOption.Deserialize();
        }

        public static OptionCollection Parse(byte[] data)
        {
            var index = MessageIndex.Options;

            Ensure.IsValid(nameof(data), data[index] != (byte)OptionCode.Pad);

            var options = new OptionCollection();

            while (data[index] != (byte)OptionCode.End)
            {
                options.Add(Parse(data, ref index));
            }

            return options;
        }

        private static IOption Parse(byte[] options, ref ushort index)
        {
            var code = options[index++];
            var length = options[index++];

            var data = new byte[length];
            Array.Copy(options, index, data, 0, length);

            index += length;

            if (IPAddressOption.IsKnownOption(code))
            {
                return new IPAddressOption(code, data);
            }

            if (MessageTypeOption.IsKnownOption(code))
            {
                return new MessageTypeOption(data);
            }

            if (StringOption.IsKnownOption(code))
            {
                return new StringOption(code, data);
            }

            if (TimeSpanOption.IsKnownOption(code))
            {
                return new TimeSpanOption(code, data);
            }

            return new UnknownOption(code, data);
        }

        public bool TryGet(byte code, [NotNullWhen(true)] out IOption? option)
        {
            option = Get(code);

            return option is not null;
        }

        public bool TryGet(OptionCode code, [NotNullWhen(true)] out IOption? option) => TryGet((byte)code, out option);
    }
}
