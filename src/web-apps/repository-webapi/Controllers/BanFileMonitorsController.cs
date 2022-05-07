using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class BanFileMonitorsController : Controller
    {
        public BanFileMonitorsController(PortalDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PortalDbContext Context { get; }

        [HttpGet]
        [Route("api/ban-file-monitors/{banFileMonitorId}")]
        public async Task<IActionResult> GetBanFileMonitor(Guid banFileMonitorId)
        {
            var banFileMonitor = await Context.BanFileMonitors
                .Include(bfm => bfm.GameServerServer)
                .SingleOrDefaultAsync(bfm => bfm.BanFileMonitorId == banFileMonitorId);

            if (banFileMonitor == null)
                return NotFound();

            return new OkObjectResult(banFileMonitor.ToDto());
        }

        [HttpGet]
        [Route("api/ban-file-monitors")]
        public async Task<IActionResult> GetBanFileMonitors(string? gameTypes, string? banFileMonitorIds, Guid? serverId, int skipEntries, int takeEntries, BanFileMonitorOrder? order)
        {
            if (order == null)
                order = BanFileMonitorOrder.BannerServerListPosition;

            var query = Context.BanFileMonitors.Include(bfm => bfm.GameServerServer).AsQueryable();

            if (serverId != null)
            {
                query = query.Where(bfm => bfm.GameServerServerId == serverId).AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(gameTypes))
            {
                var split = gameTypes.Split(",");

                var filterByGameTypes = split.Select(gt => Enum.Parse<GameType>(gt)).ToArray().Select(gt => gt.ToGameTypeInt());
                query = query.Where(bfm => filterByGameTypes.Contains(bfm.GameServerServer.GameType)).AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(banFileMonitorIds))
            {
                var split = banFileMonitorIds.Split(",");

                var filterByMonitorIds = split.Select(id => Guid.Parse(id)).ToArray();
                query = query.Where(bfm => filterByMonitorIds.Contains(bfm.BanFileMonitorId)).AsQueryable();
            }

            switch (order)
            {
                case BanFileMonitorOrder.BannerServerListPosition:
                    query = query.OrderBy(bfm => bfm.GameServerServer.BannerServerListPosition).AsQueryable();
                    break;
                case BanFileMonitorOrder.GameType:
                    query = query.OrderBy(bfm => bfm.GameServerServer.GameType).AsQueryable();
                    break;
            }

            query = query.Skip(skipEntries).AsQueryable();
            if (takeEntries != 0) query = query.Take(takeEntries).AsQueryable();

            var results = await query.ToListAsync();

            var result = results.Select(banFileMonitor => banFileMonitor.ToDto());

            return new OkObjectResult(result);
        }

        [HttpPatch]
        [Route("api/ban-file-monitors/{banFileMonitorId}")]
        public async Task<IActionResult> UpdateBanFileMonitor(Guid banFileMonitorId)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            BanFileMonitorDto banFileMonitorDto;
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                banFileMonitorDto = JsonConvert.DeserializeObject<BanFileMonitorDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            if (banFileMonitorDto == null) return new BadRequestResult();
            if (banFileMonitorDto.BanFileMonitorId != banFileMonitorId) return new BadRequestResult();

            var banFileMonitor = await Context.BanFileMonitors.Include(bfm => bfm.GameServerServer).SingleOrDefaultAsync(bfm => bfm.BanFileMonitorId == banFileMonitorDto.BanFileMonitorId);

            if (banFileMonitor == null)
                return NotFound();

            banFileMonitor.FilePath = banFileMonitorDto.FilePath;
            banFileMonitor.RemoteFileSize = banFileMonitorDto.RemoteFileSize;
            banFileMonitor.LastSync = banFileMonitorDto.LastSync;

            await Context.SaveChangesAsync();

            return new OkObjectResult(banFileMonitor.ToDto());
        }

        [HttpDelete]
        [Route("api/ban-file-monitors/{banFileMonitorId}")]
        public async Task<IActionResult> DeleteBanFileMonitor(Guid banFileMonitorId)
        {
            var banFileMonitor = await Context.BanFileMonitors
                .SingleOrDefaultAsync(bfm => bfm.BanFileMonitorId == banFileMonitorId);

            if (banFileMonitor == null)
                return NotFound();

            Context.Remove(banFileMonitor);
            await Context.SaveChangesAsync();

            return new OkResult();
        }
    }
}
