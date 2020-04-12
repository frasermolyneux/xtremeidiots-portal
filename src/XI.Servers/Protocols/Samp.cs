using System;
using System.Text;

namespace XI.Servers.Protocols
{
    public class Samp : Protocol
    {
        private readonly byte[] iQuery;

        public Samp(string host, int port)
            : base(host, port)
        {
            gameProtocol = GameProtocol.Samp;
            iQuery = new byte[]
                {(byte) 'S', (byte) 'A', (byte) 'M', (byte) 'P', 0x21, 0x21, 0x21, 0x21, 0x00, 0x00, (byte) 'i'};
            Connect(host, port);
        }

        public override void GetServerInfo()
        {
            base.GetServerInfo();
            if (!IsOnline) return;
            Query(Encoding.Default.GetString(iQuery));
            Params["passworded"] = (Response[11] == 0).ToString();

            var numPlayers = new byte[2];
            Array.Copy(Response, 12, numPlayers, 0, 2);
            Params["numplayers"] = BitConverter.ToInt16(numPlayers, 0).ToString();

            var maxPlayers = new byte[2];
            Array.Copy(Response, 14, maxPlayers, 0, 2);
            Params["maxplayers"] = BitConverter.ToInt16(maxPlayers, 0).ToString();

            var hostNameLength = new byte[4];
            Array.Copy(Response, 16, hostNameLength, 0, 4);
            var hostNameLengthInt = BitConverter.ToInt32(hostNameLength, 0);

            var hostName = new byte[hostNameLengthInt];
            Array.Copy(Response, 20, hostName, 0, hostNameLengthInt);
            Params["hostname"] = Encoding.Default.GetString(hostName);
            Params["mapname"] = "San Andreas";
        }
    }
}