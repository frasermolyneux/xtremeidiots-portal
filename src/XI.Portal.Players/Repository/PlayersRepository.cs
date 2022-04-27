using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Extensions;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Repository
{
    public class PlayersRepository : IPlayersRepository
    {
        private readonly LegacyPortalContext _legacyContext;

        public PlayersRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetPlayerListCount(PlayersFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new PlayersFilterModel();

            return await _legacyContext.Player2.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<PlayerListEntryViewModel>> GetPlayerList(PlayersFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new PlayersFilterModel();

            var players = await _legacyContext.Player2.ApplyFilter(filterModel).ToListAsync();

            var playerListEntryViewModels = players.Select(p => new PlayerListEntryViewModel
            {
                GameType = p.GameType.ToString(),
                PlayerId = p.PlayerId,
                Username = p.Username,
                Guid = p.Guid,
                IpAddress = p.IpAddress,
                FirstSeen = p.FirstSeen.ToString(CultureInfo.InvariantCulture),
                LastSeen = p.LastSeen.ToString(CultureInfo.InvariantCulture)
            }).ToList();

            return playerListEntryViewModels;
        }

        public async Task<PlayerDto> GetPlayer(Guid id)
        {
            var player = await _legacyContext.Player2.SingleAsync(p => p.PlayerId == id);

            return player?.ToDto();
        }

        public async Task<PlayerDto> GetPlayer(GameType gameType, string guid)
        {
            var player = await _legacyContext.Player2.SingleOrDefaultAsync(p => p.GameType == gameType && p.Guid == guid);

            return player?.ToDto();
        }

        public async Task<List<AliasDto>> GetPlayerAliases(Guid id)
        {
            var player = await _legacyContext.Player2
                .Include(p => p.PlayerAlias)
                .SingleAsync(p => p.PlayerId == id);

            return player.PlayerAlias.Select(alias => new AliasDto
            {
                Name = alias.Name,
                Added = alias.Added,
                LastUsed = alias.LastUsed
            }).ToList();
        }

        public async Task<List<IpAddressDto>> GetPlayerIpAddresses(Guid id)
        {
            var player = await _legacyContext.Player2
                .Include(p => p.PlayerIpAddresses)
                .SingleAsync(p => p.PlayerId == id);

            return player.PlayerIpAddresses.Select(address => new IpAddressDto
            {
                Address = address.Address,
                Added = address.Added,
                LastUsed = address.LastUsed
            }).ToList();
        }

        public async Task<List<RelatedPlayerDto>> GetRelatedPlayers(Guid id, string ipAddress)
        {
            var playerIpAddresses = await _legacyContext.PlayerIpAddresses.Include(ip => ip.PlayerPlayer)
                .Where(ip => ip.Address == ipAddress && ip.PlayerPlayerId != id)
                .ToListAsync();

            return playerIpAddresses.Select(pip => new RelatedPlayerDto
            {
                GameType = pip.PlayerPlayer.GameType.ToString(),
                Username = pip.PlayerPlayer.Username,
                PlayerId = pip.PlayerPlayer.PlayerId,
                IpAddress = pip.Address
            }).ToList();
        }

        public async Task CreatePlayer(PlayerDto playerDto)
        {
            var player = new Player2
            {
                PlayerId = Guid.NewGuid(),
                GameType = playerDto.GameType,
                Username = playerDto.Username,
                Guid = playerDto.Guid,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(player.IpAddress))
            {
                player.IpAddress = playerDto.IpAddress;

                player.PlayerIpAddresses = new List<PlayerIpAddresses>
                {
                    new PlayerIpAddresses
                    {
                        PlayerIpAddressId = Guid.NewGuid(),
                        Address = playerDto.IpAddress,
                        Added = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow
                    }
                };
            }

            player.PlayerAlias = new List<PlayerAlias>
            {
                new PlayerAlias
                {
                    PlayerAliasId = Guid.NewGuid(),
                    Name = playerDto.Username,
                    Added = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow
                }
            };

            _legacyContext.Player2.Add(player);

            await _legacyContext.SaveChangesAsync();
        }
    }
}