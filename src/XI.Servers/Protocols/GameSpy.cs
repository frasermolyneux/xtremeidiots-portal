using System;
using XI.Servers.Models;

namespace XI.Servers.Protocols
{
    public class GameSpy : Protocol
    {
        private const string QueryInfo = @"\info\";

        private const string QueryRules = @"\rules\";

        private const string QueryPlayers = @"\players\";

        public GameSpy(string host, int port)
            : base(host, port)
        {
            gameProtocol = GameProtocol.GameSpy;
            Connect(host, port);
        }

        public override string Mod => null;

        public override bool Passworded => Params.ContainsKey("password") &&
                                           (Params["password"] == "True" || Params["password"] == "1");

        public override void GetServerInfo()
        {
            base.GetServerInfo();
            if (!IsOnline) return;
            Query(QueryInfo);
            AddParams(ResponseString.Split('\\'));

            Query(QueryRules);
            AddParams(ResponseString.Split('\\'));

            Query(QueryPlayers);
            ParsePlayers();
        }

        private void ParsePlayers()
        {
            if (!IsOnline) return;

            if (Params.ContainsKey("numplayers"))
                playerCollection = new PlayerCollection();

            var parts = ResponseString.Split('\\');
            for (var i = 1; i < parts.Length; i++)
            {
                if (parts[i] == "queryid" || parts[i] == "final")
                {
                    i++;
                    continue;
                }

                var key = parts[i].Substring(0, parts[i].IndexOf("_"));
                int pNr = short.Parse(parts[i].Substring(parts[i].IndexOf("_") + 1));
                var val = parts[++i];

                if (key == "teamname") _teams.Add(val);

                try
                {
                    if (playerCollection[pNr] == null) playerCollection.Insert(pNr, new LivePlayer());
                }
                catch (ArgumentOutOfRangeException)
                {
                    playerCollection.Insert(pNr, new LivePlayer());
                }

                switch (key)
                {
                    case "playername":
                    case "player":
                        playerCollection[pNr].Name = val;
                        break;

                    case "score":
                    case "frags":
                        playerCollection[pNr].Score = short.Parse(val);
                        break;

                    case "ping":
                        playerCollection[pNr].Ping = short.Parse(val);
                        break;

                    case "team":
                        playerCollection[pNr].Team = val;
                        break;

                    default:
                        playerCollection[pNr].Parameters.Add(key, val);
                        break;
                }
            }
        }
    }
}