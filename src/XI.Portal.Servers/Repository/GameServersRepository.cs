using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Extensions;

namespace XI.Portal.Servers.Repository
{
    public class GameServersRepository : IGameServersRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IGameServersRepositoryOptions _options;

        public GameServersRepository(IGameServersRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options;
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<List<GameServers>> GetGameServers(ClaimsPrincipal user)
        {
            return await _legacyContext.GameServers.ApplyAuthPolicies(user).OrderBy(server => server.BannerServerListPosition).ToListAsync();
        }

        public async Task<GameServers> GetGameServer(Guid? id, ClaimsPrincipal user)
        {
            return await _legacyContext.GameServers
                .ApplyAuthPolicies(user)
                .FirstOrDefaultAsync(m => m.ServerId == id);
        }

        public async Task CreateGameServer(GameServers model)
        {
            model.ServerId = Guid.NewGuid();
            model.LiveLastUpdated = DateTime.UtcNow;

            _legacyContext.Add(model);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateGameServer(Guid? id, GameServers model, ClaimsPrincipal user)
        {
            var storedModel = await GetGameServer(id, user);
            storedModel.Title = model.Title;
            storedModel.Hostname = model.Hostname;
            storedModel.QueryPort = model.QueryPort;
            storedModel.FtpHostname = model.FtpHostname;
            storedModel.FtpUsername = model.FtpUsername;
            storedModel.FtpPassword = model.FtpPassword;
            storedModel.RconPassword = model.RconPassword;
            storedModel.ShowOnBannerServerList = model.ShowOnBannerServerList;
            storedModel.BannerServerListPosition = model.BannerServerListPosition;
            storedModel.HtmlBanner = model.HtmlBanner;
            storedModel.ShowOnPortalServerList = model.ShowOnPortalServerList;
            storedModel.ShowChatLog = model.ShowChatLog;

            _legacyContext.Update(storedModel);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> GameServerExists(Guid id, ClaimsPrincipal user)
        {
            return await _legacyContext.GameServers.ApplyAuthPolicies(user).AnyAsync(e => e.ServerId == id);
        }

        public async Task RemoveGameServer(Guid id, ClaimsPrincipal user)
        {
            var model = await GetGameServer(id, user);

            _legacyContext.GameServers.Remove(model);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<List<GameServers>> GetGameServersForCredentials(ClaimsPrincipal user)
        {
            return await _legacyContext.GameServers.ApplyCredentialAuthPolicies(user).OrderBy(server => server.BannerServerListPosition).ToListAsync();
        }
    }
}