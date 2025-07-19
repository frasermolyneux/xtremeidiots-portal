using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Controllers;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing game server credentials display and access
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessCredentials)]
    public class CredentialsController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

        /// <summary>
        /// Initializes a new instance of the CredentialsController
        /// </summary>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="repositoryApiClient">Client for repository API operations</param>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <param name="configuration">Configuration service for app settings</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public CredentialsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<CredentialsController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Displays the credentials index page with filtered game servers based on user permissions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The credentials index view with game servers</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to access credentials</exception>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                // Authorization is handled at the controller level with [Authorize(Policy = AuthPolicies.AccessCredentials)]
                // Get user's game permissions for filtering
                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.FtpCredentials, UserProfileClaimType.RconCredentials };
                var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

                Logger.LogInformation("User {UserId} querying game servers for credentials with {GameTypeCount} game types and {GameServerIdCount} specific servers",
                    User.XtremeIdiotsId(), gameTypes?.Length ?? 0, gameServerIds?.Length ?? 0);

                // Retrieve game servers based on user permissions
                var gameServersList = await GetAuthorizedGameServersAsync(gameTypes, gameServerIds, cancellationToken);
                if (gameServersList == null)
                {
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                // Apply credential-specific authorization for each server
                await ApplyCredentialAuthorizationAsync(gameServersList, cancellationToken);

                TrackSuccessTelemetry("CredentialsViewed", "Index", new Dictionary<string, string>
                {
                    { "Controller", "Credentials" },
                    { "Resource", "GameServerCredentials" },
                    { "Context", "CredentialsDisplay" },
                    { "GameServerCount", gameServersList.Count.ToString() }
                });

                Logger.LogInformation("User {UserId} successfully viewed credentials for {GameServerCount} game servers",
                    User.XtremeIdiotsId(), gameServersList.Count);

                return View(gameServersList);
            }, "Display credentials index page with game server credentials");
        }

        /// <summary>
        /// Retrieves game servers that the user is authorized to view credentials for
        /// </summary>
        /// <param name="gameTypes">Game types the user has access to</param>
        /// <param name="gameServerIds">Specific game server IDs the user has access to</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of game servers or null if API call failed</returns>
        private async Task<List<GameServerDto>?> GetAuthorizedGameServersAsync(GameType[]? gameTypes, Guid[]? gameServerIds, CancellationToken cancellationToken)
        {
            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
                gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition, cancellationToken);

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items == null)
            {
                Logger.LogWarning("Failed to retrieve game servers for credentials view for user {UserId} - API response status: {IsSuccess}",
                    User.XtremeIdiotsId(), gameServersApiResponse.IsSuccess);

                // Track API failure as a custom event rather than an error
                TelemetryClient.TrackEvent("CredentialsApiFailure", new Dictionary<string, string>
                {
                    { "ApiSuccess", gameServersApiResponse.IsSuccess.ToString() },
                    { "Controller", "Credentials" },
                    { "Action", "GetAuthorizedGameServersAsync" },
                    { "UserId", User.XtremeIdiotsId()?.ToString() ?? "Unknown" }
                });

                return null;
            }

            return gameServersApiResponse.Result.Data.Items.ToList();
        }

        /// <summary>
        /// Applies credential-specific authorization to each game server, clearing credentials for unauthorized access
        /// </summary>
        /// <param name="gameServersList">List of game servers to check authorization for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task ApplyCredentialAuthorizationAsync(List<GameServerDto> gameServersList, CancellationToken cancellationToken)
        {
            foreach (var gameServerDto in gameServersList)
            {
                // Check FTP credential authorization
                var ftpResource = new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.GameServerId);
                var canViewFtpCredential = await authorizationService.AuthorizeAsync(User, ftpResource, AuthPolicies.ViewFtpCredential);

                if (!canViewFtpCredential.Succeeded)
                {
                    TrackUnauthorizedAccessAttempt("ViewFtpCredential", "FtpCredential",
                        $"GameType:{gameServerDto.GameType},GameServerId:{gameServerDto.GameServerId}", gameServerDto);
                    gameServerDto.ClearFtpCredentials();
                }

                // Check RCON credential authorization
                var canViewRconCredential = await authorizationService.AuthorizeAsync(User, ftpResource, AuthPolicies.ViewRconCredential);

                if (!canViewRconCredential.Succeeded)
                {
                    TrackUnauthorizedAccessAttempt("ViewRconCredential", "RconCredential",
                        $"GameType:{gameServerDto.GameType},GameServerId:{gameServerDto.GameServerId}", gameServerDto);
                    gameServerDto.ClearRconCredentials();
                }
            }
        }
    }
}