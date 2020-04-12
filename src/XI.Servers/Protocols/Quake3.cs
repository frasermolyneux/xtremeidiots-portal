using System;
using System.Text.RegularExpressions;
using XI.Servers.Models;

namespace XI.Servers.Protocols
{
    public class Quake3 : Protocol
    {
        private const string PlayerRegex = "(?<score>.+) (?<ping>.+) \\\"(?<name>.+)\\\"";

        private const string QueryStatus = @"ÿÿÿÿgetstatus";

        public Quake3(string host, int port)
            : base(host, port)
        {
            gameProtocol = GameProtocol.Quake3;
            Connect(host, port);
        }

        public override string Name => !ServerOnline ? null : Params["sv_hostname"];

        public override string Mod => !ServerOnline ? null : Params["fs_game"];

        public override int MaxPlayers
        {
            get
            {
                try
                {
                    return short.Parse(Params["sv_maxclients"]);
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override bool Passworded
        {
            get
            {
                if (!ServerOnline) return false;
                return Params.ContainsKey("g_needpass") && Params["g_needpass"] == "0";
            }
        }

        public override void GetServerInfo()
        {
            base.GetServerInfo();
            if (!IsOnline) return;
            Query(QueryStatus);

            if (ResponseString.IndexOf("disconnect", StringComparison.Ordinal) != -1)
            {
                ServerOnline = false;
                return;
            }

            var lines = ResponseString.Substring(3).Split('\n');
            if (lines.Length < 2)
            {
                ServerOnline = false;
                return;
            }

            AddParams(lines[1].Split('\\'));

            if (lines.Length <= 2) return;
            for (var i = 2; i < lines.Length; i++)
            {
                if (lines[i].Length == 0) continue;
                playerCollection.Add(ParsePlayer(lines[i]));
            }
        }

        private static LivePlayer ParsePlayer(string playerInfo)
        {
            var regPattern = new Regex(PlayerRegex);
            var regMatch = regPattern.Match(playerInfo);
            return new LivePlayer(regMatch.Groups["name"].Value, int.Parse(regMatch.Groups["score"].Value),
                int.Parse(regMatch.Groups["ping"].Value));
        }
    }
}