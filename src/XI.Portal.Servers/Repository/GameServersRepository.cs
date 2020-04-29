using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public class GameServersRepository : IGameServersRepository
    {
        private readonly LegacyPortalContext _legacyContext;

        public GameServersRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetGameServersCount(GameServerFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            return await _legacyContext.GameServers.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<GameServerDto>> GetGameServers(GameServerFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            var gameServers = await _legacyContext.GameServers
                .ApplyFilter(filterModel)
                .ToListAsync();

            var models = gameServers.Select(s => s.ToDto()).ToList();

            return models;
        }

        public async Task<GameServerDto> GetGameServer(Guid gameServerId)
        {
            var gameServer = await _legacyContext.GameServers
                .SingleOrDefaultAsync(s => s.ServerId == gameServerId);

            return gameServer?.ToDto();
        }

        public async Task CreateGameServer(GameServerDto gameServerDto)
        {
            if (gameServerDto == null) throw new ArgumentNullException(nameof(gameServerDto));

            var gameServer = new GameServers
            {
                ServerId = Guid.NewGuid(),
                Title = gameServerDto.Title,
                GameType = gameServerDto.GameType,
                Hostname = gameServerDto.Hostname,
                QueryPort = gameServerDto.QueryPort,
                FtpHostname = gameServerDto.FtpHostname,
                FtpUsername = gameServerDto.FtpUsername,
                FtpPassword = gameServerDto.FtpPassword,
                RconPassword = gameServerDto.RconPassword,
                ShowOnBannerServerList = gameServerDto.ShowOnBannerServerList,
                BannerServerListPosition = gameServerDto.BannerServerListPosition,
                HtmlBanner = gameServerDto.HtmlBanner,
                ShowOnPortalServerList = gameServerDto.ShowOnPortalServerList,
                ShowChatLog = gameServerDto.ShowChatLog,
#pragma warning disable 618 // Required to prevent SQL error
                LiveLastUpdated = DateTime.UtcNow
#pragma warning restore 618
            };

            _legacyContext.GameServers.Add(gameServer);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateGameServer(GameServerDto gameServerDto)
        {
            if (gameServerDto == null) throw new ArgumentNullException(nameof(gameServerDto));

            var gameServer = await _legacyContext.GameServers.SingleOrDefaultAsync(s => s.ServerId == gameServerDto.ServerId);

            if (gameServer == null)
                throw new NullReferenceException(nameof(gameServer));

            gameServer.Title = gameServerDto.Title;
            gameServer.Hostname = gameServerDto.Hostname;
            gameServer.QueryPort = gameServerDto.QueryPort;
            gameServer.FtpHostname = gameServerDto.FtpHostname;
            gameServer.FtpUsername = gameServerDto.FtpUsername;
            gameServer.FtpPassword = gameServerDto.FtpPassword;
            gameServer.RconPassword = gameServerDto.RconPassword;
            gameServer.ShowOnBannerServerList = gameServerDto.ShowOnBannerServerList;
            gameServer.BannerServerListPosition = gameServerDto.BannerServerListPosition;
            gameServer.HtmlBanner = gameServerDto.HtmlBanner;
            gameServer.ShowOnPortalServerList = gameServerDto.ShowOnPortalServerList;
            gameServer.ShowChatLog = gameServerDto.ShowChatLog;

            await _legacyContext.SaveChangesAsync();
        }

        public async Task DeleteGameServer(Guid gameServerId)
        {
            var gameServer = await _legacyContext.GameServers
                .SingleOrDefaultAsync(s => s.ServerId == gameServerId);

            if (gameServer == null)
                throw new NullReferenceException(nameof(gameServer));

            _legacyContext.Remove(gameServer);
            await _legacyContext.SaveChangesAsync();
        }
    }
}