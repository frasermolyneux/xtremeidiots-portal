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
    public class RconMonitorsRepository : IRconMonitorsRepository
    {
        private readonly LegacyPortalContext _legacyContext;

        public RconMonitorsRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetRconMonitorsCount(RconMonitorFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            return await _legacyContext.RconMonitors.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<RconMonitorDto>> GetRconMonitors(RconMonitorFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            var rconMonitors = await _legacyContext.RconMonitors
                .ApplyFilter(filterModel)
                .ToListAsync();

            var models = rconMonitors.Select(rm => rm.ToDto()).ToList();

            return models;
        }

        public async Task<RconMonitorDto> GetRconMonitor(Guid rconMonitorId)
        {
            var rconMonitor = await _legacyContext.RconMonitors
                .Include(rm => rm.GameServerServer)
                .SingleOrDefaultAsync(rm => rm.RconMonitorId == rconMonitorId);

            return rconMonitor?.ToDto();
        }

        public async Task CreateRconMonitor(RconMonitorDto rconMonitorDto)
        {
            if (rconMonitorDto == null) throw new ArgumentNullException(nameof(rconMonitorDto));

            var server = await _legacyContext.GameServers.SingleOrDefaultAsync(s => s.ServerId == rconMonitorDto.ServerId);

            if (server == null)
                throw new NullReferenceException(nameof(server));

            var rconMonitor = new RconMonitors
            {
                RconMonitorId = Guid.NewGuid(),
                LastUpdated = DateTime.UtcNow,
                MonitorMapRotation = rconMonitorDto.MonitorMapRotation,
                MapRotationLastUpdated = DateTime.UtcNow,
                MonitorPlayers = rconMonitorDto.MonitorPlayers,
                MonitorPlayerIps = rconMonitorDto.MonitorPlayerIps,
                //LastError = string.Empty;
                GameServerServer = server
            };

            _legacyContext.RconMonitors.Add(rconMonitor);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateRconMonitor(RconMonitorDto rconMonitorDto)
        {
            if (rconMonitorDto == null) throw new ArgumentNullException(nameof(rconMonitorDto));

            var rconMonitor = await _legacyContext.RconMonitors.SingleOrDefaultAsync(rm => rm.RconMonitorId == rconMonitorDto.RconMonitorId);

            if (rconMonitor == null)
                throw new NullReferenceException(nameof(rconMonitor));

            rconMonitor.MonitorMapRotation = rconMonitorDto.MonitorMapRotation;
            rconMonitor.MonitorPlayers = rconMonitorDto.MonitorPlayers;
            rconMonitor.MonitorPlayerIps = rconMonitorDto.MonitorPlayerIps;

            await _legacyContext.SaveChangesAsync();
        }

        public async Task DeleteRconMonitor(Guid rconMonitorId)
        {
            var rconMonitor = await _legacyContext.RconMonitors
                .SingleOrDefaultAsync(fm => fm.RconMonitorId == rconMonitorId);

            if (rconMonitor == null)
                throw new NullReferenceException(nameof(rconMonitor));

            _legacyContext.Remove(rconMonitor);
            await _legacyContext.SaveChangesAsync();
        }

        //public async Task<List<RconMonitorStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        //{
        //    var results = new List<RconMonitorStatusViewModel>();

        //    var rconMonitors = await GetRconMonitors(user, requiredClaims);

        //    foreach (var rconMonitor in rconMonitors)
        //        try
        //        {
        //            var rconClient = _rconClientFactory.CreateInstance(
        //                rconMonitor.GameServerServer.GameType,
        //                rconMonitor.GameServerServer.ServerId,
        //                rconMonitor.GameServerServer.Hostname,
        //                rconMonitor.GameServerServer.QueryPort,
        //                rconMonitor.GameServerServer.RconPassword
        //            );

        //            var commandResult = rconClient.GetPlayers();

        //            var errorMessage = string.Empty;

        //            if (rconMonitor.LastUpdated < DateTime.UtcNow.AddMinutes(-15))
        //                errorMessage = "ERROR - The rcon status has not been updated in the past 15 minutes";


        //            results.Add(new RconMonitorStatusViewModel
        //            {
        //                RconMonitor = rconMonitor,
        //                GameServer = rconMonitor.GameServerServer,
        //                RconStatusResult = $"Total players: {commandResult.Count}",
        //                ErrorMessage = errorMessage
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            results.Add(new RconMonitorStatusViewModel
        //            {
        //                RconMonitor = rconMonitor,
        //                GameServer = rconMonitor.GameServerServer,
        //                ErrorMessage = ex.Message
        //            });
        //        }

        //    return results;
        //}
    }
}