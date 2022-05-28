namespace XtremeIdiots.CodDemos.Huffman
{
    internal partial class HuffmanTree
    {
        private class Node
        {
            public byte Symbol { get; set; }
            public int Frequency { get; set; }
            public Node? Right { get; set; }
            public Node? Left { get; set; }

            public bool IsLeaf => Left == null && Right == null;

            public Node Get(bool bit)
            {
                return (bit ? Right : Left) ?? this;
            }
        }
    }
}