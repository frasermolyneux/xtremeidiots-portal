using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class BanFileMonitorsController : Controller
    {
        private readonly ILogger<BanFileMonitorsController> logger;
        private readonly PortalDbContext context;

        public BanFileMonitorsController(
            ILogger<BanFileMonitorsController> logger,
            PortalDbContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        [Route("api/ban-file-monitors/{banFileMonitorId}")]
        public async Task<IActionResult> GetBanFileMonitor(Guid banFileMonitorId)
        {
            var banFileMonitor = await context.BanFileMonitors
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

            var query = context.BanFileMonitors.Include(bfm => bfm.GameServerServer).AsQueryable();

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
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (banFileMonitorDto == null) return new BadRequestResult();
            if (banFileMonitorDto.BanFileMonitorId != banFileMonitorId) return new BadRequestResult();

            var banFileMonitor = await context.BanFileMonitors.Include(bfm => bfm.GameServerServer).SingleOrDefaultAsync(bfm => bfm.BanFileMonitorId == banFileMonitorDto.BanFileMonitorId);

            if (banFileMonitor == null)
                return NotFound();

            banFileMonitor.FilePath = banFileMonitorDto.FilePath;
            banFileMonitor.RemoteFileSize = banFileMonitorDto.RemoteFileSize;
            banFileMonitor.LastSync = banFileMonitorDto.LastSync;

            await context.SaveChangesAsync();

            return new OkObjectResult(banFileMonitor.ToDto());
        }

        [HttpDelete]
        [Route("api/ban-file-monitors/{banFileMonitorId}")]
        public async Task<IActionResult> DeleteBanFileMonitor(Guid banFileMonitorId)
        {
            var banFileMonitor = await context.BanFileMonitors
                .SingleOrDefaultAsync(bfm => bfm.BanFileMonitorId == banFileMonitorId);

            if (banFileMonitor == null)
                return NotFound();

            context.Remove(banFileMonitor);
            await context.SaveChangesAsync();

            return new OkResult();
        }
    }
}
