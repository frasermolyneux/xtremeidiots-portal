using System;
using System.Text;
using XI.Servers.Models;

namespace XI.Servers.Protocols
{
    public class GameSpy2 : Protocol
    {
        private readonly string playerinfos = Encoding.Default.GetString(new byte[]
        {
            0x00, 0xFF, 0x00
        });

        private readonly string queryBase = Encoding.Default.GetString(new byte[]
        {
            0xFE, 0xFD, 0x00, 0x04, 0x05, 0x06, 0x07
        });

        private readonly string seperator = Encoding.Default.GetString(new byte[]
        {
            0x00, 0x00
        });

        private readonly string serverinfos = Encoding.Default.GetString(new byte[]
        {
            0xFF, 0x00, 0x00
        });

        private readonly string teaminfos = Encoding.Default.GetString(new byte[]
        {
            0x00, 0x00, 0xFF
        });

        public GameSpy2(string host, int port)
            : base(host, port)
        {
            gameProtocol = GameProtocol.GameSpy2;
            Connect(host, port);
        }

        public override string Mod => Params["all_active_mods"];

        public override bool Passworded => Params.ContainsKey("password") && Params["password"] == "0";

        public override void GetServerInfo()
        {
            base.GetServerInfo();
            if (!IsOnline) return;
            Query(queryBase + serverinfos);
            ParseDetails();

            Query(queryBase + playerinfos);
            ParsePlayers();

            Query(queryBase + teaminfos);
            ParseTeam();
        }

        private void ParseDetails()
        {
            if (!IsOnline) return;
            Offset = 5;

            while (Offset < Response.Length)
            {
                var key = ReadNextParam();
                var val = ReadNextParam();
                if (key.Length == 0) continue;
                Params[key] = val;
            }
        }

        private void ParsePlayers()
        {
            if (!IsOnline) return;
            Offset = ResponseString.IndexOf(seperator, StringComparison.Ordinal) + 2;

            while (Offset < Response.Length)
            {
                var pNr = playerCollection.Add(new LivePlayer());
                playerCollection[pNr].Name = ReadNextParam();
                playerCollection[pNr].Score = int.Parse(ReadNextParam());
                playerCollection[pNr].Parameters.Add("deaths", ReadNextParam());
                playerCollection[pNr].Ping = int.Parse(ReadNextParam());
                playerCollection[pNr].Team = ReadNextParam();
                playerCollection[pNr].Parameters.Add("kills", ReadNextParam());
            }
        }

        private void ParseTeam()
        {
            if (!IsOnline) return;
            Offset = ResponseString.IndexOf(seperator, 7, StringComparison.Ordinal) + 2;

            _teams.Add(ReadNextParam());
            ReadNextParam();
            _teams.Add(ReadNextParam());
        }
    }
}