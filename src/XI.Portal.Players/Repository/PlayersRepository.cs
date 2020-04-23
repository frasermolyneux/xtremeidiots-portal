using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
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

            return await filterModel.ApplyFilter(_legacyContext).CountAsync();
        }

        public async Task<List<PlayerListEntryViewModel>> GetPlayerList(PlayersFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new PlayersFilterModel();

            var players = await filterModel.ApplyFilter(_legacyContext).ToListAsync();

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

        public async Task<PlayerDto> GetPlayer(Guid id, ClaimsPrincipal user, string[] requiredClaims)
        {
            var player = await _legacyContext.Player2.SingleAsync(p => p.PlayerId == id);

            return new PlayerDto
            {
                PlayerId = player.PlayerId,
                GameType = player.GameType,
                Username = player.Username,
                Guid = player.Guid,
                IpAddress = player.IpAddress,
                FirstSeen = player.FirstSeen,
                LastSeen = player.LastSeen
            };
        }

        public async Task<List<AliasDto>> GetPlayerAliases(Guid id, ClaimsPrincipal user, string[] requiredClaims)
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

        public async Task<List<IpAddressDto>> GetPlayerIpAddresses(Guid id, ClaimsPrincipal user, string[] requiredClaims)
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

        public async Task<List<RelatedPlayerDto>> GetRelatedPlayers(Guid id, string ipAddress, ClaimsPrincipal user, string[] requiredClaims)
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
    }
}