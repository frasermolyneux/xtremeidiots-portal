using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy;
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
    }
}