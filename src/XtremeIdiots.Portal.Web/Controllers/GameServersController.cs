using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Manages game server administration and configuration
/// </summary>
/// <remarks>
/// Initializes a new instance of the GameServersController
/// </remarks>
/// <param name="authorizationService">Authorization service for policy-based access control</param>
/// <param name="repositoryApiClient">Repository API client for data access</param>
/// <param name="telemetryClient">Application Insights telemetry client</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
[Authorize(Policy = AuthPolicies.AccessGameServers)]
public class GameServersController(
    IAuthorizationService authorizationService,
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<GameServersController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Displays a list of game servers accessible to the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with list of game servers</returns>
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
    /// Displays the create game server form
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with create game server form</returns>
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            AddGameTypeViewData();
            return await Task.FromResult(View(new GameServerViewModel()));
        }, nameof(Create));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GameServerViewModel model, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var modelValidationResult = CheckModelState(model, m => AddGameTypeViewData(m.GameType));
            if (modelValidationResult is not null)
                return modelValidationResult;

#pragma warning disable CS8604
            var createGameServerDto = new CreateGameServerDto(model.Title, model.GameType, model.Hostname, model.QueryPort);
#pragma warning restore CS8604

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                createGameServerDto.GameType,
                AuthPolicies.CreateGameServer,
                nameof(Create),
                "GameServer",
                $"GameType:{createGameServerDto.GameType}");

            if (authResult is not null)
                return authResult;

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

            if (authResult is not null)
                return authResult;

            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            gameServerData.ClearNoPermissionBanFileMonitors(gameTypes, banFileMonitorIds);

            return View(gameServerData);
        }, nameof(Details));
    }

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

            if (authResult != null)
                return authResult;

            var canEditGameServerFtp = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerFtp);
            if (!canEditGameServerFtp.Succeeded)
                gameServerData.ClearFtpCredentials();

            var canEditGameServerRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerRcon);
            if (!canEditGameServerRcon.Succeeded)
                gameServerData.ClearRconCredentials();

            return View(gameServerData.ToViewModel());
        }, nameof(Edit));
    }

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
            if (modelValidationResult is not null)
                return modelValidationResult;

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                gameServerData.GameType,
                AuthPolicies.EditGameServer,
                nameof(Edit),
                "GameServer",
                $"GameType:{gameServerData.GameType},GameServerId:{model.GameServerId}",
                gameServerData);

            if (authResult != null)
                return authResult;

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

            var canDeleteGameServer = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);
            if (!canDeleteGameServer.Succeeded)
            {
                TrackUnauthorizedAccessAttempt(nameof(Delete), "GameServer", $"GameType:{gameServerData.GameType},GameServerId:{id}", gameServerData);
                return Unauthorized();
            }

            return View(gameServerData.ToViewModel());
        }, nameof(Delete));
    }

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

    private void AddGameTypeViewData(GameType? selected = null)
    {
        try
        {
            selected ??= GameType.Unknown;

            var gameTypes = User.GetGameTypesForGameServers();
            ViewData[nameof(GameType)] = new SelectList(gameTypes, selected);

            Logger.LogDebug("Added {GameTypeCount} game types to ViewData with {SelectedGameType} selected",
                gameTypes.Count, selected);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding game type ViewData for user {UserId}", User.XtremeIdiotsId());

            ViewData[nameof(GameType)] = new SelectList(Enumerable.Empty<GameType>(), selected ?? GameType.Unknown);
        }
    }
}