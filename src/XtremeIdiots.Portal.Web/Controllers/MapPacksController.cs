using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

[Authorize(Policy = AuthPolicies.ManageMaps)]
public class MapPacksController : BaseController
{
 private readonly IAuthorizationService authorizationService;
 private readonly IRepositoryApiClient repositoryApiClient;
 private readonly IServersApiClient serversApiClient;

 public MapPacksController(
 IAuthorizationService authorizationService,
 IRepositoryApiClient repositoryApiClient,
 IServersApiClient serversApiClient,
 TelemetryClient telemetryClient,
 ILogger<MapPacksController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
 this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
 this.serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));
 }

 [HttpGet]
 public async Task<IActionResult> Create(Guid gameServerId, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
 gameServerId,
 AuthPolicies.CreateMapPack,
 nameof(Create),
 cancellationToken);

 if (actionResult != null) return actionResult;

 ViewData["GameServer"] = gameServerData;

 return View(new CreateMapPackViewModel
 {
 GameServerId = gameServerId,
 Title = string.Empty,
 Description = string.Empty,
 GameMode = string.Empty
 });
 }, nameof(Create));
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Create(CreateMapPackViewModel model, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
 model.GameServerId,
 AuthPolicies.CreateMapPack,
 nameof(Create),
 cancellationToken);

 if (actionResult != null) return actionResult;

 var modelValidationResult = CheckModelState(model, m =>
 {
 ViewData["GameServer"] = gameServerData;
 });
 if (modelValidationResult != null) return modelValidationResult;

 var createMapPackDto = new CreateMapPackDto(model.GameServerId, model.Title, model.Description)
 {
 GameMode = model.GameMode,
 SyncToGameServer = model.SyncToGameServer
 };

 var createMapPackApiResponse = await repositoryApiClient.MapPacks.V1.CreateMapPack(createMapPackDto, cancellationToken);

 if (!createMapPackApiResponse.IsSuccess)
 {
 if (createMapPackApiResponse.Result?.Errors != null)
 {
 foreach (var error in createMapPackApiResponse.Result.Errors)
 {
 ModelState.AddModelError(error.Target ?? string.Empty, error.Message ?? "An error occurred");
 }
 }
 else
 {
 ModelState.AddModelError(string.Empty, "An error occurred while creating the map pack");
 }

 ViewData["GameServer"] = gameServerData;
 return View(model);
 }

 TrackSuccessTelemetry(nameof(Create), "MapPackCreated", new Dictionary<string, string>
 {
 { nameof(model.GameServerId), model.GameServerId.ToString() },
 { nameof(model.Title), model.Title },
 { "GameType", gameServerData!.GameType.ToString() }
 });

 this.AddAlertSuccess($"Map pack '{model.Title}' has been created successfully for {gameServerData.Title}.");

 return RedirectToAction("Manage", "MapManager", new { id = model.GameServerId });
 }, nameof(Create));
 }

 private async Task<(IActionResult? ActionResult, GameServerDto? GameServerData)> GetAuthorizedGameServerAsync(
 Guid gameServerId,
 string policy,
 string action,
 CancellationToken cancellationToken = default)
 {
 var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(gameServerId, cancellationToken);

 if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Game server {GameServerId} not found when {Action} map pack", gameServerId, action);
 return (NotFound(), null);
 }

 var gameServerData = gameServerApiResponse.Result.Data;
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 gameServerData.GameType,
 policy,
 action,
 "MapPack",
 $"GameType:{gameServerData.GameType},GameServerId:{gameServerId}",
 gameServerData);

 return authResult is not null ? (authResult, null) : (null, gameServerData);
 }
}