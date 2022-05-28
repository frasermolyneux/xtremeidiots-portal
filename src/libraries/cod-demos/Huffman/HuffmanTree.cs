using System.Collections;

namespace XtremeIdiots.CodDemos.Huffman
{
    internal partial class HuffmanTree
    {
        public HuffmanTree(int[] frequencies)
        {
            if (frequencies == null) throw new ArgumentNullException(nameof(frequencies));
            if (frequencies.Length != 256)
                throw new ArgumentException("Array must contain 256 entries", nameof(frequencies));

            var nodes = new List<Node>(Enumerable.Range(0, 256).Select(i => new Node
            {
                Symbol = (byte)i,
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

        private Node? Root { get; }

        public byte[] Decode(BitArray bits)
        {
            if (Root == null)
                throw new ArgumentNullException(nameof(Root));

            var currentRoot = Root;
            var decoded = new List<byte>();

            foreach (bool bit in bits)
            {
                currentRoot = currentRoot.Get(bit);

                // If this node is a leaf, take it's value and start again at the root.
                if (!currentRoot.IsLeaf) continue;

                decoded.Add(currentRoot.Symbol);
                currentRoot = Root;
            }

            return decoded.ToArray();
        }
    }
}