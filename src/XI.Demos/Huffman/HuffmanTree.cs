using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XI.Demos.Huffman
{
    internal class HuffmanTree
    {
        public HuffmanTree(int[] frequencies)
        {
            if (frequencies == null) throw new ArgumentNullException(nameof(frequencies));
            if (frequencies.Length != 256)
                throw new ArgumentException("Array must contain 256 entries", nameof(frequencies));

            var nodes = new List<Node>(Enumerable.Range(0, 256).Select(i => new Node
            {
                Symbol = (byte) i,
                Frequency = frequencies[i]
            }));

            while (nodes.Count > 1)
            {
                var orderedNodes = nodes.OrderBy(node => node.Frequency).ToArray();

                if (orderedNodes.Length >= 2)
                {
                    // Take first two items.
                    var taken = orderedNodes.Take(2).ToArray();

                    // Create a parent node by combining the frequencies.
                    var parent = new Node
                    {
                        Symbol = 0,
                        Frequency = taken[0].Frequency + taken[1].Frequency,
                        Left = taken[0],
                        Right = taken[1]
                    };

                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }

                Root = nodes.FirstOrDefault();
            }
        }

        private Node Root { get; }

        public byte[] Decode(BitArray bits)
        {
            var current = Root;
            var decoded = new List<byte>();

            foreach (bool bit in bits)
            {
                current = current.Get(bit);

                // If this node is a leaf, take it's value and start again at the root.
                if (!current.IsLeaf) continue;

                decoded.Add(current.Symbol);
                current = Root;
            }

            return decoded.ToArray();
        }

        private class Node
        {
            public byte Symbol { get; set; }
            public int Frequency { get; set; }
            public Node Right { get; set; }
            public Node Left { get; set; }

            public bool IsLeaf => Left == null && Right == null;

            public Node Get(bool bit)
            {
                return (bit ? Right : Left) ?? this;
            }
        }
    }
}