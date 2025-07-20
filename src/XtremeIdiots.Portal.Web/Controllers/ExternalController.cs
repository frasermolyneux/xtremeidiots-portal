using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

public class ExternalController : BaseController
{
 private readonly IRepositoryApiClient repositoryApiClient;

 public ExternalController(
 IRepositoryApiClient repositoryApiClient,
 TelemetryClient telemetryClient,
 ILogger<ExternalController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
 }

 [HttpGet]
 public async Task<IActionResult> LatestAdminActions(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 Logger.LogInformation("External request for latest admin actions view");

 var adminActionDtos = await repositoryApiClient.AdminActions.V1.GetAdminActions(
 null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

 if (!adminActionDtos.IsSuccess || adminActionDtos.Result?.Data is null)
 {
 Logger.LogWarning("Failed to retrieve admin actions for external view - API response unsuccessful or null");
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController).Replace("Controller", ""), new { id = 500 });
 }

 Logger.LogInformation("Successfully retrieved {Count} admin actions for external view",
 adminActionDtos.Result.Data.Items?.Count() ?? 0);

 TrackSuccessTelemetry("LatestAdminActionsViewed", nameof(LatestAdminActions), new Dictionary<string, string>
 {
 { "Controller", nameof(ExternalController).Replace("Controller", "") },
 { "Resource", "AdminActionsView" },
 { "Count", (adminActionDtos.Result.Data.Items?.Count() ?? 0).ToString() }
 });

 return View(adminActionDtos);
 }, nameof(LatestAdminActions));
 }

 [HttpGet]
 [EnableCors("CorsPolicy")]
 public IActionResult GetLatestAdminActions(CancellationToken cancellationToken = default)
 {
 return RedirectPermanent("/External/GetLatestAdminActions");
 }
}