using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing game servers across multiple Call of Duty games.
/// Provides CRUD operations for game server configuration including network settings, 
/// authentication credentials and server feature toggles.
/// Handles complex authorization requirements where users have different permission
/// levels across different games, ensuring proper access control and audit trails.
/// Supports both banner integration and portal listing features.
/// </summary>
[Authorize(Policy = AuthPolicies.AccessGameServers)]
public class GameServersController : BaseController
{
 private readonly IAuthorizationService authorizationService;
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the GameServersController with required dependencies
 /// </summary>
 /// <param name="authorizationService">Service for checking user authorization policies</param>
 /// <param name="repositoryApiClient">Client for accessing game server data through the Repository API</param>
 /// <param name="telemetryClient">Client for tracking application telemetry and analytics</param>
 /// <param name="logger">Logger instance for this controller</param>
 /// <param name="configuration">Application configuration settings</param>
 /// <exception cref="ArgumentNullException">Thrown when required dependencies are null</exception>
 public GameServersController(
 IAuthorizationService authorizationService,
 IRepositoryApiClient repositoryApiClient,
 TelemetryClient telemetryClient,
 ILogger<GameServersController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
 this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
 }

 /// <summary>
 /// Displays the list of game servers that the current user has access to view.
 /// Filters servers based on user claims for specific games and server permissions.
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The index view with a list of accessible game servers, or error response if retrieval fails</returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionaccess game servers</exception>
 [HttpGet]
 public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameServer };
 var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

 var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
 gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition, cancellationToken);

 if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items is null)
 {
 Logger.LogWarning("Failed to retrieve game servers for user {UserId}", User.XtremeIdiotsId());
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController).Replace("Controller", ""), new { id = 500 });
 }

 var gameServerCount = gameServersApiResponse.Result.Data.Items.Count();
 Logger.LogInformation("User {UserId} successfully accessed {GameServerCount} game servers",
 User.XtremeIdiotsId(), gameServerCount);

 return View(gameServersApiResponse.Result.Data.Items);
 }, nameof(Index));
 }

 /// <summary>
 /// Displays the creation form for a new game server with appropriate game type options
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The create game server view with an empty form and available game types</returns>
 [HttpGet]
 public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 AddGameTypeViewData();
 return await Task.FromResult(View(new GameServerViewModel()));
 }, nameof(Create));
 }

 /// <summary>
 /// Creates a new game server based on the submitted form data.
 /// Validates user permissions and conditionally includes FTP and RCON credentials
 /// based on authorization levels. Supports configuration of tracking, banners and bot features.
 /// </summary>
 /// <param name="model">The game server view model containing the server details and configuration</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Redirects to index on success, or view with validation errors</returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate game servers for the specified game type</exception>
 /// <exception cref="ArgumentNullException">Thrown when model validation fails</exception>
 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Create(GameServerViewModel model, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var modelValidationResult = CheckModelState(model, m => AddGameTypeViewData(m.GameType));
 if (modelValidationResult is not null) return modelValidationResult;

#pragma warning disable CS8604 // Possible null reference argument. // ModelState check is just above.
 var createGameServerDto = new CreateGameServerDto(model.Title, model.GameType, model.Hostname, model.QueryPort);
#pragma warning restore CS8604 // Possible null reference argument.

 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 createGameServerDto.GameType,
 AuthPolicies.CreateGameServer,
 nameof(Create),
 "GameServer",
 $"GameType:{createGameServerDto.GameType}");

 if (authResult is not null) return authResult;

 createGameServerDto.Title = model.Title;
 createGameServerDto.Hostname = model.Hostname;
 createGameServerDto.QueryPort = model.QueryPort;

 var canEditGameServerFtp = await authorizationService.AuthorizeAsync(User, createGameServerDto.GameType, AuthPolicies.EditGameServerFtp);
 if (canEditGameServerFtp.Succeeded)
 {
 createGameServerDto.FtpHostname = model.FtpHostname;
 createGameServerDto.FtpPort = model.FtpPort;
 createGameServerDto.FtpUsername = model.FtpUsername;
 createGameServerDto.FtpPassword = model.FtpPassword;
 }

 var canEditGameServerRcon = await authorizationService.AuthorizeAsync(User, createGameServerDto.GameType, AuthPolicies.EditGameServerRcon);
 if (canEditGameServerRcon.Succeeded)
 createGameServerDto.RconPassword = model.RconPassword;

 createGameServerDto.LiveTrackingEnabled = model.LiveTrackingEnabled;
 createGameServerDto.BannerServerListEnabled = model.BannerServerListEnabled;
 createGameServerDto.ServerListPosition = model.ServerListPosition;
 createGameServerDto.HtmlBanner = model.HtmlBanner;
 createGameServerDto.PortalServerListEnabled = model.PortalServerListEnabled;
 createGameServerDto.ChatLogEnabled = model.ChatLogEnabled;
 createGameServerDto.BotEnabled = model.BotEnabled;

 var createResult = await repositoryApiClient.GameServers.V1.CreateGameServer(createGameServerDto, cancellationToken);

 if (createResult.IsSuccess)
 {
 TrackSuccessTelemetry("GameServerCreated", nameof(Create), new Dictionary<string, string>
 {
 { nameof(GameType), createGameServerDto.GameType.ToString() },
 { nameof(CreateGameServerDto.Title), createGameServerDto.Title ?? "Unknown" }
 });

 this.AddAlertSuccess($"The game server has been successfully created for {model.GameType}");
 return RedirectToAction(nameof(Index));
 }
 else
 {
 Logger.LogWarning("Failed to create game server for user {UserId} and game type {GameType}",
 User.XtremeIdiotsId(), model.GameType);

 this.AddAlertDanger("Failed to create the game server. Please try again.");
 AddGameTypeViewData(model.GameType);
 return View(model);
 }
 }, "CreatePost");
 }

 /// <summary>
 /// Displays detailed information for a specific game server including configuration,
 /// associated ban file monitors and operational settings.
 /// Filters sensitive information based on user permissions.
 /// </summary>
 /// <param name="id">The unique identifier of the game server to display details for</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The details view with comprehensive game server information, or appropriate error response</returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionview the game server</exception>
 /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
 [HttpGet]
 public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

 if (gameServerApiResponse.IsNotFound)
 {
 Logger.LogWarning("Game server {GameServerId} not found when viewing details", id);
 return NotFound();
 }

 if (gameServerApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Game server data is null for {GameServerId}", id);
 return BadRequest();
 }

 var gameServerData = gameServerApiResponse.Result.Data;
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 gameServerData.GameType,
 AuthPolicies.ViewGameServer,
 nameof(Details),
 "GameServer",
 $"GameType:{gameServerData.GameType},GameServerId:{id}",
 gameServerData);

 if (authResult is not null) return authResult;

 var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
 var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

 gameServerData.ClearNoPermissionBanFileMonitors(gameTypes, banFileMonitorIds);

 return View(gameServerData);
 }, nameof(Details));
 }

 /// <summary>
 /// Displays the edit form for modifying an existing game server.
 /// Populates form with current server data and conditionally clears sensitive credentials
 /// based on user permissions for FTP and RCON access.
 /// </summary>
 /// <param name="id">The unique identifier of the game server to edit</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The edit game server view with populated form data, or appropriate error response</returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionedit the game server</exception>
 /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
 [HttpGet]
 public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

 if (gameServerApiResponse.IsNotFound)
 {
 Logger.LogWarning("Game server {GameServerId} not found when editing", id);
 return NotFound();
 }

 if (gameServerApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Game server data is null for {GameServerId}", id);
 return BadRequest();
 }

 var gameServerData = gameServerApiResponse.Result.Data;
 AddGameTypeViewData(gameServerData.GameType);

 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 gameServerData.GameType,
 AuthPolicies.EditGameServer,
 nameof(Edit),
 "GameServer",
 $"GameType:{gameServerData.GameType},GameServerId:{id}",
 gameServerData);

 if (authResult != null) return authResult;

 var canEditGameServerFtp = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerFtp);
 if (!canEditGameServerFtp.Succeeded)
 gameServerData.ClearFtpCredentials();

 var canEditGameServerRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerRcon);
 if (!canEditGameServerRcon.Succeeded)
 gameServerData.ClearRconCredentials();

 return View(gameServerData.ToViewModel());
 }, nameof(Edit));
 }

 /// <summary>
 /// Updates an existing game server based on the submitted form data.
 /// Validates user permissions and conditionally updates FTP and RCON credentials
 /// based on authorization levels. Handles all server configuration and feature toggles.
 /// </summary>
 /// <param name="model">The game server view model containing the updated server details and configuration</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Redirects to index on success, or view with validation errors</returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionedit the game server</exception>
 /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
 /// <exception cref="ArgumentNullException">Thrown when model validation fails</exception>
 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(GameServerViewModel model, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId, cancellationToken);

 if (gameServerApiResponse.IsNotFound)
 {
 Logger.LogWarning("Game server {GameServerId} not found when updating", model.GameServerId);
 return NotFound();
 }

 if (gameServerApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Game server data is null for {GameServerId}", model.GameServerId);
 return BadRequest();
 }

 var gameServerData = gameServerApiResponse.Result.Data;

 var modelValidationResult = CheckModelState(model, m => AddGameTypeViewData(m.GameType));
 if (modelValidationResult is not null) return modelValidationResult;

 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 gameServerData.GameType,
 AuthPolicies.EditGameServer,
 nameof(Edit),
 "GameServer",
 $"GameType:{gameServerData.GameType},GameServerId:{model.GameServerId}",
 gameServerData);

 if (authResult != null) return authResult;

 var editGameServerDto = new EditGameServerDto(gameServerData.GameServerId)
 {
 Title = model.Title,
 Hostname = model.Hostname,
 QueryPort = model.QueryPort
 };

 var canEditGameServerFtp = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerFtp);
 if (canEditGameServerFtp.Succeeded)
 {
 editGameServerDto.FtpHostname = model.FtpHostname;
 editGameServerDto.FtpPort = model.FtpPort;
 editGameServerDto.FtpUsername = model.FtpUsername;
 editGameServerDto.FtpPassword = model.FtpPassword;
 }

 var canEditGameServerRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerRcon);
 if (canEditGameServerRcon.Succeeded)
 editGameServerDto.RconPassword = model.RconPassword;

 editGameServerDto.LiveTrackingEnabled = model.LiveTrackingEnabled;
 editGameServerDto.BannerServerListEnabled = model.BannerServerListEnabled;
 editGameServerDto.ServerListPosition = model.ServerListPosition;
 editGameServerDto.HtmlBanner = model.HtmlBanner;
 editGameServerDto.PortalServerListEnabled = model.PortalServerListEnabled;
 editGameServerDto.ChatLogEnabled = model.ChatLogEnabled;
 editGameServerDto.BotEnabled = model.BotEnabled;

 var updateResult = await repositoryApiClient.GameServers.V1.UpdateGameServer(editGameServerDto, cancellationToken);

 if (updateResult.IsSuccess)
 {
 TrackSuccessTelemetry("GameServerUpdated", nameof(Edit), new Dictionary<string, string>
 {
 { nameof(GameServerDto.GameServerId), gameServerData.GameServerId.ToString() },
 { nameof(GameType), gameServerData.GameType.ToString() },
 { nameof(GameServerDto.Title), gameServerData.Title ?? "Unknown" }
 });

 this.AddAlertSuccess($"The game server {gameServerData.Title} has been updated for {gameServerData.GameType}");
 return RedirectToAction(nameof(Index));
 }
 else
 {
 Logger.LogWarning("Failed to update game server {GameServerId} for user {UserId}",
 model.GameServerId, User.XtremeIdiotsId());

 this.AddAlertDanger("Failed to update the game server. Please try again.");
 AddGameTypeViewData(model.GameType);
 return View(model);
 }
 }, "EditPost");
 }

 /// <summary>
 /// Displays the delete confirmation form for a game server.
 /// Shows server details and requests confirmation before permanent deletion.
 /// Requires elevated permissions as deletion is a destructive operation.
 /// </summary>
 /// <param name="id">The unique identifier of the game server to delete</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The delete confirmation view with game server information, or appropriate error response</returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissiondelete game servers</exception>
 /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
 [HttpGet]
 public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

 if (gameServerApiResponse.IsNotFound)
 {
 Logger.LogWarning("Game server {GameServerId} not found when deleting", id);
 return NotFound();
 }

 if (gameServerApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Game server data is null for {GameServerId}", id);
 return BadRequest();
 }

 var gameServerData = gameServerApiResponse.Result.Data;

 // Use direct authorization check since DeleteGameServer doesn't require a resource
 var canDeleteGameServer = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);
 if (!canDeleteGameServer.Succeeded)
 {
 TrackUnauthorizedAccessAttempt(nameof(Delete), "GameServer", $"GameType:{gameServerData.GameType},GameServerId:{id}", gameServerData);
 return Unauthorized();
 }

 return View(gameServerData.ToViewModel());
 }, nameof(Delete));
 }

 /// <summary>
 /// Permanently deletes a game server after confirmation.
 /// This is a destructive operation that removes all server configuration and history.
 /// Requires elevated permissions and logs the action for audit purposes.
 /// </summary>
 /// <param name="id">The unique identifier of the game server to delete</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Redirects to index on success, or appropriate error response</returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissiondelete game servers</exception>
 /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
 [HttpPost]
 [ActionName(nameof(Delete))]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

 if (gameServerApiResponse.IsNotFound)
 {
 Logger.LogWarning("Game server {GameServerId} not found when confirming deletion", id);
 return NotFound();
 }

 if (gameServerApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Game server data is null for {GameServerId}", id);
 return BadRequest();
 }

 var gameServerData = gameServerApiResponse.Result.Data;

 // Use direct authorization check since DeleteGameServer doesn't require a resource
 var canDeleteGameServer = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);
 if (!canDeleteGameServer.Succeeded)
 {
 TrackUnauthorizedAccessAttempt(nameof(Delete), "GameServer", $"GameType:{gameServerData.GameType},GameServerId:{id}", gameServerData);
 return Unauthorized();
 }

 var deleteResult = await repositoryApiClient.GameServers.V1.DeleteGameServer(id, cancellationToken);

 if (deleteResult.IsSuccess)
 {
 TrackSuccessTelemetry("GameServerDeleted", nameof(Delete), new Dictionary<string, string>
 {
 { nameof(GameServerDto.GameServerId), gameServerData.GameServerId.ToString() },
 { nameof(GameType), gameServerData.GameType.ToString() },
 { nameof(GameServerDto.Title), gameServerData.Title ?? "Unknown" }
 });

 this.AddAlertSuccess($"The game server {gameServerData.Title} has been deleted for {gameServerData.GameType}");
 return RedirectToAction(nameof(Index));
 }
 else
 {
 Logger.LogWarning("Failed to delete game server {GameServerId} for user {UserId}",
 id, User.XtremeIdiotsId());

 this.AddAlertDanger("Failed to delete the game server. Please try again.");
 return RedirectToAction(nameof(Index));
 }
 }, nameof(DeleteConfirmed));
 }

 /// <summary>
 /// Adds game type options to ViewData for form dropdown selection.
 /// Retrieves available game types based on user's game server permissions
 /// and provides fallback handling for error scenarios.
 /// </summary>
 /// <param name="selected">The currently selected game type, defaults to Unknown if not specified</param>
 private void AddGameTypeViewData(GameType? selected = null)
 {
 try
 {
 if (selected is null)
 selected = GameType.Unknown;

 var gameTypes = User.GetGameTypesForGameServers();
 ViewData[nameof(GameType)] = new SelectList(gameTypes, selected);

 Logger.LogDebug("Added {GameTypeCount} game types to ViewData with {SelectedGameType} selected",
 gameTypes.Count(), selected);
 }
 catch (Exception ex)
 {
 Logger.LogError(ex, "Error adding game type ViewData for user {UserId}", User.XtremeIdiotsId());

 // Fallback to empty list to prevent view errors
 ViewData[nameof(GameType)] = new SelectList(Enumerable.Empty<GameType>(), selected ?? GameType.Unknown);
 }
 }
}
