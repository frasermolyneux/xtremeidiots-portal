using XI.Portal.Data.Legacy.Models;
using XI.Portal.Players.Dto;

namespace XI.Portal.Players.Extensions
{
    public static class PlayerExtensions
    {
        public static PlayerDto ToDto(this Player2 player)
        {
            var playerDto = new PlayerDto
            {
                PlayerId = player.PlayerId,
                GameType = player.GameType,
                Username = player.Username,
                Guid = player.Guid,
                IpAddress = player.IpAddress,
                FirstSeen = player.FirstSeen,
                LastSeen = player.LastSeen
            };

            return playerDto;
        }
    }
}