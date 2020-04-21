using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XI.Servers.Interfaces.Models;

namespace XI.Servers.Models
{
    public class GameServerPlayer : IGameServerPlayer
    {
        private readonly IQueryPlayer _queryPlayer;

        public GameServerPlayer(IQueryPlayer queryPlayer)
        {
            _queryPlayer = queryPlayer ?? throw new ArgumentNullException(nameof(queryPlayer));
        }

        public IRconPlayer RconPlayer { get; set; }

        public string Num => RconPlayer != null ? RconPlayer.Num : "";
        public string Guid => RconPlayer != null ? RconPlayer.Guid : "";
        public string Name => _queryPlayer != null ? _queryPlayer.Name : "";
        public string IpAddress => RconPlayer != null ? RconPlayer.IpAddress : "";
        public int Score => _queryPlayer?.Score ?? 0;
        public string Rate => RconPlayer != null ? RconPlayer.Rate : "";

        public string NormalizedName
        {
            get
            {
                var toRemove = new List<string> {"^0", "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9"};

                var toReturn = Name.ToUpper();
                toReturn = toRemove.Aggregate(toReturn, (current, val) => current.Replace(val, ""));

                if (toReturn.StartsWith("["))
                {
                    var regex = new Regex("^(\\[.*\\])");
                    var match = regex.Match(toReturn);

                    if (match.Success)
                    {
                        var matchedTag = match.Groups[1];
                        toReturn = toReturn.Replace(matchedTag.ToString(), "");
                    }
                }

                toReturn = toReturn.Trim();
                return toReturn;
            }
        }
    }
}