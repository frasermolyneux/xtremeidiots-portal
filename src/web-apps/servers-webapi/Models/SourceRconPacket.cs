using System.Text;

namespace XtremeIdiots.Portal.ServersWebApi.Models
{
    internal class SourceRconPacket
    {
        public SourceRconPacket(int id, int type, string body)
        {
            Id = id;
            Type = type;
            Body = body;
        }

        public int Size // 32-bit little-endian Signed Integer
        {
            get
            {
                // 4 bytes for ID
                // 4 bytes for Type
                // At least 1 byte for Body (including null terminator)
                // 1 byte for null terminator

                var bodySize = Encoding.Default.GetBytes(Body).Length + 1;
                return 4 + 4 + bodySize + 1;
            }
        }

        public int Id { get; } // 32-bit little-endian Signed Integer
        public int Type { get; } // 32-bit little-endian Signed Integer
        public string Body { get; } // Null-terminated ASCII String

        public byte[] PacketBytes
        {
            get
            {
                var size = BitConverter.GetBytes(Size);
                var id = BitConverter.GetBytes(Id);
                var type = BitConverter.GetBytes(Type);
                var body = Encoding.ASCII.GetBytes(Body);
                var terminator = new byte[] { 0x00 };

                var packet = new byte[4 + Size];

                var pointer = 0;
                size.CopyTo(packet, pointer);
                pointer += 4;

                id.CopyTo(packet, pointer);
                pointer += 4;

                type.CopyTo(packet, pointer);
                pointer += 4;

                body.CopyTo(packet, pointer);
                pointer += body.Length;
                terminator.CopyTo(packet, pointer);
                pointer++;
                terminator.CopyTo(packet, pointer);

                return packet;
            }
        }
    }
}