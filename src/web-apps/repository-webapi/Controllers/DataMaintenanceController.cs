using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Data.Legacy;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class DataMaintenanceController : ControllerBase
{
    public DataMaintenanceController(LegacyPortalContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public LegacyPortalContext Context { get; }

    [HttpDelete]
    [Route("api/DataMaintenance/PruneChatMessages")]
    public async Task<IActionResult> PruneChatMessages()
    {
        //await Context.Database.ExecuteSqlRawAsync(
        //    $"DELETE FROM [dbo].[ChatMessages] WHERE [Timestamp] < CAST('{DateTime.UtcNow.AddMonths(-6):yyyy-MM-dd} 12:00:00' AS date)");

        return new OkResult();
    }

    [HttpDelete]
    [Route("api/DataMaintenance/PruneGameServerEvents")]
    public async Task<IActionResult> PruneGameServerEvents()
    {
        //await Context.Database.ExecuteSqlRawAsync(
        //    $"DELETE FROM [dbo].[GameServerEvents] WHERE [Timestamp] < CAST('{DateTime.UtcNow.AddMonths(-6):yyyy-MM-dd} 12:00:00' AS date)");

        return new OkResult();
    }
}