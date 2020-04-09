using System;
using System.Collections;
using XI.Demos.Huffman;

namespace XI.Demos.Models
{
    internal struct DemoMessage
    {
        private int _bit;
        private int _readcount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DemoMessage" /> struct.
        /// </summary>
        /// <param name="data">The data of the message.</param>
        /// <exception cref="ArgumentNullException">Thrown if data is null.</exception>
        public DemoMessage(byte[] data)
            : this()
        {
            if (data == null) throw new ArgumentNullException("data");

            Data = data;
            CurrentSize = data.Length;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DemoMessage" /> struct.
        /// </summary>
        /// <param name="length">The length of the data of the message.</param>
        /// <exception cref="ArgumentOutOfRangeException">length must be greater than 0.</exception>
        public DemoMessage(int length)
            : this()
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", length, "must be greater than 0");

            Data = new byte[length];
            CurrentSize = length;
        }

        public int CurrentSize { get; set; }
        public byte[] Data { get; set; }

        public bool IsAtEndOfData => _readcount >= CurrentSize;

        /// <summary>
        ///     Decodes this message using the specified huffman tree.
        /// </summary>
        /// <param name="huffmanTree">The huffman tree to use.</param>
        /// <returns>The decoded message.</returns>
        public DemoMessage Decode(HuffmanTree huffmanTree)
        {
            return new DemoMessage(huffmanTree.Decode(new BitArray(Data)));
        }

        /// <summary>
        ///     Reads the specified number of <paramref name="bits" /> from this message.
        /// </summary>
        /// <param name="bits">The bits to read from this message.</param>
        /// <returns>The read value.</returns>
        /// <exception cref="Exception">Read failure.</exception>
        public int ReadBits(int bits)
        {
            var value = 0;
            var readBits = 0;
            while (bits > 0)
            {
                if (IsAtEndOfData)
                    throw new Exception("Read failure");

                // Pick the remaining bits of the current byte, up to the required number of bits.
                var curbits = Math.Min(bits, 8 - _bit);

                // Read the bits from the data and shift them into the result.
                var read = Data[_readcount] & ((int) Math.Pow(2, curbits) - 1);

                value <<= readBits;
                value |= read;

                // Keep track of read bits.
                bits -= curbits;
                readBits += curbits;

                _bit += curbits;

                // If the bit index is at the end of the byte, move the bit index to the start of the next file.
                if (_bit != 8) continue;
                _bit = 0;
                _readcount++;
            }

            return value;
        }

        /// <summary>
        ///     Reads a byte from this message.
        /// </summary>
        /// <returns>The read byte.</returns>
        /// <exception cref="Exception">Read failure.</exception>
        public int ReadByte()
        {
            return ReadBits(8);
        }

        /// <summary>
        ///     Reads a short from this message.
        /// </summary>
        /// <returns>The read short.</returns>
        /// <exception cref="Exception">Read failure.</exception>
        public int ReadInt16()
        {
            return ReadBits(16);
        }

        /// <summary>
        ///     Reads a int from this message.
        /// </summary>
        /// <returns>The read int.</returns>
        /// <exception cref="Exception">Read failure.</exception>
        public int ReadInt32()
        {
            return ReadBits(32);
        }

        /// <summary>
        ///     Reads a string from this message.
        /// </summary>
        /// <returns>The read string.</returns>
        /// <exception cref="Exception">Read failure.</exception>
        public string ReadString()
        {
            var buffer = string.Empty;

            while (!IsAtEndOfData)
            {
                var c = ReadByte();
                if (c == 0)
                    break;

                //                buffer += c > 127 ? '.' : (char)c;
                buffer += (char) c;
            }

            return buffer;
        }

        /// <summary>
        ///     Reads the specified number of <paramref name="bits" /> from this message.
        /// </summary>
        /// <param name="bits">The bits to read from this message.</param>
        /// <returns>The read value.</returns>
        /// <exception cref="Exception">Read failure.</exception>
        public int ReadAlignedBits(int bits)
        {
            // TODO: This method is broken (doesn't take bit count into account)
            // It does however not matter for our purposes.
            return ReadBits(8);
        }
    }
}