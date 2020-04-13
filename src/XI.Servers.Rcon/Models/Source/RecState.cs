namespace XI.Servers.Rcon.Models.Source
{
    internal class RecState
    {
        public int BytesSoFar;
        public byte[] Data;
        public bool IsPacketLength;

        public int PacketCount;
        public int PacketLength;

        internal RecState()
        {
            PacketLength = -1;
            BytesSoFar = 0;
            IsPacketLength = false;
        }
    }
}