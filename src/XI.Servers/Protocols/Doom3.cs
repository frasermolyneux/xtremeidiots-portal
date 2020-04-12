using System;
using System.Text;
using XI.Servers.Models;

namespace XI.Servers.Protocols
{
    public class Doom3 : Protocol
    {
        private const string QueryStatus = @"ÿÿgetInfo";

        public Doom3(string host, int port)
            : base(host, port)
        {
            gameProtocol = GameProtocol.Doom3;
            Connect(host, port);
        }

        public override string Name => Params["si_name"];

        public override string Mod => Params["gamename"];

        public override string Map => Params["si_map"];

        public override int MaxPlayers => short.Parse(Params["si_maxPlayers"]);

        public override bool Passworded => !Params.ContainsKey("si_usepass") || Params["si_usepass"] != "0";

        public override void GetServerInfo()
        {
            base.GetServerInfo();
            if (!IsOnline) return;
            Query(QueryStatus);

            Offset = 23;
            var playerOffset = ResponseString.IndexOf(Encoding.Default.GetString(new byte[]
            {
                0, 0, 0, 0
            }), StringComparison.Ordinal);

            playerOffset += 3;

            while (Offset < playerOffset)
            {
                var key = ReadNextParam();
                var val = ReadNextParam();

                if (key.Length == 0) continue;
                Params[key] = val;
            }

            Offset = playerOffset;

            while (Offset < Response.Length)
                try
                {
                    var pNr = playerCollection.Add(new LivePlayer());
                    playerCollection[pNr].Parameters.Add("playernr", Response[Offset++].ToString());
                    playerCollection[pNr].Ping = BitConverter.ToInt16(Response, Offset);
                    Offset += 2;
                    playerCollection[pNr].Score = BitConverter.ToInt16(Response, Offset);
                    Offset += 4;
                    playerCollection[pNr].Name = ReadNextParam();

                    if (Offset + 7 > Response.Length) break;
                }
                catch (ArgumentException)
                {
                    break;
                }
        }
    }
}