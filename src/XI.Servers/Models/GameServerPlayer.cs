using System;
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

        public string NormalizedName => Name.Normalize();
    }
}