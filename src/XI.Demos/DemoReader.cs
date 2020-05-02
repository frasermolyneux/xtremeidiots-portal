using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XI.CommonTypes;
using XI.Demos.Huffman;
using XI.Demos.Models;

namespace XI.Demos
{
    internal class DemoReader
    {
        private readonly Stream _demoStream;
        private readonly GameType _gameVersion;
        private readonly HuffmanTree _huffmanTree;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DemoReader" /> class.
        /// </summary>
        /// <param name="demoStream">The demo stream.</param>
        /// <param name="gameVersion">The game type.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if demoStream or extension is null.
        /// </exception>
        public DemoReader(Stream demoStream, GameType gameVersion)
        {
            _demoStream = demoStream ?? throw new ArgumentNullException(nameof(demoStream));
            _gameVersion = gameVersion;

            // Initialize the huffman tree.
            _huffmanTree =
                new HuffmanTree(gameVersion == GameType.CallOfDuty2
                    ? HuffmanFrequencies.Quake3
                    : HuffmanFrequencies.CallOfDuty4);

            // Skip first byte.
            if (_gameVersion == GameType.CallOfDuty4 || _gameVersion == GameType.CallOfDuty5)
                _demoStream.Seek(1, SeekOrigin.Begin);
        }


        /// <summary>
        ///     Reads the configuration rules form the demo file.
        /// </summary>
        /// <returns>A collection of configuration rules.</returns>
        public IDictionary<string, string> ReadConfiguration()
        {
            var message = ReadDemoMessage();

            // Skip a byte of the message.
            message.ReadByte();

            var rules = ParseGameState(message);

            return
                rules.Where(r => r.Item2.StartsWith("\\"))
                    .SelectMany(r => SplitConfigString(r.Item2))
                    .GroupBy(r => r.Key)
                    .Select(g => g.First())
                    .ToDictionary(r => r.Key, r => r.Value);
        }

        private DemoMessage ReadDemoMessage()
        {
            // Read the sequence number.
            ReadFromStream();

            // Read the length.
            var length = ReadFromStream();

            var compressedMessage = new DemoMessage(length);

            // The message starts with an unknown value. Skip this value.
            ReadFromStream();
            compressedMessage.CurrentSize -= 4;

            // Fill the message contents from the stream.
            ReadFromStream(compressedMessage.Data, compressedMessage.CurrentSize);

            // Decode the message contents.
            return compressedMessage.Decode(_huffmanTree);
        }

        private static IDictionary<string, string> SplitConfigString(string input, char seperator = '\\')
        {
            var result = new Dictionary<string, string>();
            string key = null;

            foreach (var value in input.Split(seperator))
            {
                if (key == null)
                {
                    if (!string.IsNullOrEmpty(value))
                        key = value;

                    continue;
                }

                result[key] = value;
                key = null;
            }

            return result;
        }

        private IEnumerable<Tuple<int, string>> ParseGameState(DemoMessage demoMessage)
        {
            // Skip sequence number. We don't need it.
            demoMessage.ReadInt32();

            var idx = -1;
            while (!demoMessage.IsAtEndOfData)
            {
                var command = demoMessage.ReadByte();

                // Iterate trough strings in message until no longer in the config command.
                if (command != 2)
                    break;

                var stringIdx = demoMessage.ReadInt16();

                if (_gameVersion == GameType.CallOfDuty2)
                {
                    idx = stringIdx;
                    stringIdx = 1;
                }

                while (stringIdx > 0)
                {
                    if (demoMessage.IsAtEndOfData)
                        break;

                    if (_gameVersion != GameType.CallOfDuty2)
                    {
                        if (demoMessage.ReadAlignedBits(1) != 0)
                            idx++;
                        else
                            idx = demoMessage.ReadAlignedBits(12);
                    }

                    var value = demoMessage.ReadString();

                    // TODO: Find the right terminator... waiting for stringIdx to be 0 causes troubles
                    if (value == "fffffffffffffffffffffffffffff300")
                        break;

                    yield return new Tuple<int, string>(idx, value);
                    stringIdx--;
                }
            }
        }

        private int ReadFromStream()
        {
            var buffer = new byte[4];
            ReadFromStream(buffer, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        private void ReadFromStream(byte[] buffer, int count)
        {
            if (!_demoStream.CanRead)
                throw new Exception("Unable to read from demo stream");

            // Fill the buffer from the current position within the demo stream
            var numRead = _demoStream.Read(buffer, 0, count);

            if (numRead != count)
                throw new Exception("Unexpected end of stream");
        }
    }
}