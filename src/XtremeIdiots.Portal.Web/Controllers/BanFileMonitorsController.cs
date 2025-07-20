using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.BanFileMonitors;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

[Authorize(Policy = AuthPolicies.AccessBanFileMonitors)]
public class BanFileMonitorsController(
 IAuthorizationService authorizationService,
 IRepositoryApiClient repositoryApiClient,
 TelemetryClient telemetryClient,
 ILogger<BanFileMonitorsController> logger,
 IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
 private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
 private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

 [HttpGet]
 public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
 var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

 var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition, cancellationToken);

 if (!banFileMonitorsApiResponse.IsSuccess || banFileMonitorsApiResponse.Result?.Data?.Items is null)
 {
 Logger.LogError("Failed to retrieve ban file monitors for user {UserId}", User.XtremeIdiotsId());
 return RedirectToAction("Display", "Errors", new { id = 500 });
 }

 return View(banFileMonitorsApiResponse.Result.Data.Items);
 }, nameof(Index));
 }

 [HttpGet]
 public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 await AddGameServersViewData(cancellationToken: cancellationToken);
 return View(new CreateBanFileMonitorViewModel { FilePath = string.Empty });
 }, nameof(Create));
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Create(CreateBanFileMonitorViewModel model, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId, cancellationToken);

 if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Game server {GameServerId} not found when creating ban file monitor", model.GameServerId);
 return NotFound();
 }

 var gameServerData = gameServerApiResponse.Result.Data;

 var modelValidationResult = await CheckModelStateAsync(model, async m =>
 {
 await AddGameServersViewData(model.GameServerId, cancellationToken);
 m.GameServer = gameServerData;
 });
 if (modelValidationResult is not null) return modelValidationResult;

 var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 authorizationResource,
 AuthPolicies.CreateBanFileMonitor,
 nameof(Create),
 "BanFileMonitor",
 $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}",
 gameServerData);

 if (authResult is not null) return authResult;

 var createBanFileMonitorDto = new CreateBanFileMonitorDto(model.GameServerId, model.FilePath, gameServerData.GameType);
 await repositoryApiClient.BanFileMonitors.V1.CreateBanFileMonitor(createBanFileMonitorDto, cancellationToken);

 TrackSuccessTelemetry("BanFileMonitorCreated", nameof(Create), new Dictionary<string, string>
 {
 { nameof(model.GameServerId), model.GameServerId.ToString() },
 { nameof(model.FilePath), model.FilePath },
 { nameof(GameType), gameServerData.GameType.ToString() }
 });

 this.AddAlertSuccess($"The ban file monitor has been created for {gameServerData.Title}");

 return RedirectToAction(nameof(Index));
 }, nameof(Create));
 }

 [HttpGet]
 public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 id, AuthPolicies.ViewBanFileMonitor, nameof(Details), cancellationToken);

 if (actionResult is not null) return actionResult;

 return View(banFileMonitorData);
 }, nameof(Details));
 }

 [HttpGet]
 public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 id, AuthPolicies.EditBanFileMonitor, nameof(Edit), cancellationToken);

 if (actionResult is not null) return actionResult;

 await AddGameServersViewData(banFileMonitorData!.GameServerId, cancellationToken);

 var viewModel = new EditBanFileMonitorViewModel
 {
 BanFileMonitorId = banFileMonitorData.BanFileMonitorId,
 FilePath = banFileMonitorData.FilePath,
 RemoteFileSize = banFileMonitorData.RemoteFileSize,
 LastSync = banFileMonitorData.LastSync,
 GameServerId = banFileMonitorData.GameServerId,
 GameServer = banFileMonitorData.GameServer
 };

 return View(viewModel);
 }, nameof(Edit));
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(EditBanFileMonitorViewModel model, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 model.BanFileMonitorId, AuthPolicies.EditBanFileMonitor, nameof(Edit), cancellationToken);

 if (actionResult is not null) return actionResult;

 var modelValidationResult = await CheckModelStateAsync(model, async m =>
 {
 await AddGameServersViewData(model.GameServerId, cancellationToken);
 model.GameServer = banFileMonitorData!.GameServer;
 });
 if (modelValidationResult is not null) return modelValidationResult;

 var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorData!.BanFileMonitorId, model.FilePath);
 await repositoryApiClient.BanFileMonitors.V1.UpdateBanFileMonitor(editBanFileMonitorDto, cancellationToken);

 TrackSuccessTelemetry("BanFileMonitorUpdated", nameof(Edit), new Dictionary<string, string>
 {
 { nameof(model.BanFileMonitorId), model.BanFileMonitorId.ToString() },
 { nameof(model.FilePath), model.FilePath },
 { nameof(GameType), banFileMonitorData.GameServer.GameType.ToString() }
 });

 this.AddAlertSuccess($"The ban file monitor has been updated for {banFileMonitorData.GameServer.Title}");

 return RedirectToAction(nameof(Index));
 }, nameof(Edit));
 }

 [HttpGet]
 public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 id, AuthPolicies.DeleteBanFileMonitor, nameof(Delete), cancellationToken);

 if (actionResult is not null) return actionResult;

 await AddGameServersViewData(banFileMonitorData!.GameServerId, cancellationToken);

 return View(banFileMonitorData);
 }, nameof(Delete));
 }

 [HttpPost]
 [ActionName("Delete")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 id, AuthPolicies.DeleteBanFileMonitor, nameof(DeleteConfirmed), cancellationToken);

 if (actionResult is not null) return actionResult;

 await repositoryApiClient.BanFileMonitors.V1.DeleteBanFileMonitor(id, cancellationToken);

 TrackSuccessTelemetry("BanFileMonitorDeleted", nameof(DeleteConfirmed), new Dictionary<string, string>
 {
 { nameof(id), id.ToString() },
 { nameof(GameType), banFileMonitorData!.GameServer.GameType.ToString() },
 { "GameServerId", banFileMonitorData.GameServer.GameServerId.ToString() }
 });

 this.AddAlertSuccess($"The ban file monitor has been deleted for {banFileMonitorData.GameServer.Title}");

 return RedirectToAction(nameof(Index));
 }, nameof(DeleteConfirmed));
 }

 private async Task AddGameServersViewData(Guid? selected = null, CancellationToken cancellationToken = default)
 {
 try
 {
 var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
 var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

 var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition, cancellationToken);

 if (gameServersApiResponse.Result?.Data?.Items is not null)
 {
 ViewData["GameServers"] = new SelectList(gameServersApiResponse.Result.Data.Items, nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title), selected);
 }
 else
 {
 Logger.LogWarning("Failed to load game servers for user {UserId}", User.XtremeIdiotsId());
 ViewData["GameServers"] = new SelectList(Enumerable.Empty<GameServerDto>(), nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title));
 }
 }
 catch (Exception ex)
 {
 Logger.LogError(ex, "Error loading game servers view data for user {UserId}", User.XtremeIdiotsId());
 ViewData["GameServers"] = new SelectList(Enumerable.Empty<GameServerDto>(), nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title));
 }
 }

 private async Task<(IActionResult? ActionResult, BanFileMonitorDto? BanFileMonitor)> GetAuthorizedBanFileMonitorAsync(
 Guid id,
 string policy,
 string action,
 CancellationToken cancellationToken = default)
 {
 var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id, cancellationToken);

 if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result?.Data?.GameServer is null)
 {
 Logger.LogWarning("Ban file monitor {BanFileMonitorId} not found when {Action}", id, action);
 return (NotFound(), null);
 }

 var banFileMonitorData = banFileMonitorApiResponse.Result.Data;
 var gameServerData = banFileMonitorData.GameServer;

 var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 authorizationResource,
 policy,
 action,
 "BanFileMonitor",
 $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}",
 banFileMonitorData);

 if (authResult is not null)
 {
 return (authResult, null);
 }

 return (null, banFileMonitorData);
 }
}