using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing game server credentials display and access
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessCredentials)]
    public class CredentialsController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<CredentialsController> logger;

        /// <summary>
        /// Initializes a new instance of the CredentialsController
        /// </summary>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="repositoryApiClient">Client for repository API operations</param>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public CredentialsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<CredentialsController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the credentials index page with filtered game servers based on user permissions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The credentials index view with game servers</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to view credentials",
                    User.XtremeIdiotsId());

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.FtpCredentials, UserProfileClaimType.RconCredentials };
                var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

                logger.LogInformation("User {UserId} querying game servers for credentials with {GameTypeCount} game types and {GameServerIdCount} specific servers",
                    User.XtremeIdiotsId(), gameTypes?.Length ?? 0, gameServerIds?.Length ?? 0);

                var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

                if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve game servers for credentials view for user {UserId} - API response status: {IsSuccess}",
                        User.XtremeIdiotsId(), gameServersApiResponse.IsSuccess);

                    var apiFailureTelemetry = new EventTelemetry("CredentialsApiFailure")
                        .Enrich(User);
                    apiFailureTelemetry.Properties.TryAdd("ApiSuccess", gameServersApiResponse.IsSuccess.ToString());
                    telemetryClient.TrackEvent(apiFailureTelemetry);

                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var gameServersList = gameServersApiResponse.Result.Data.Items.ToList();

                foreach (var gameServerDto in gameServersList)
                {
                    var canViewFtpCredential = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.GameServerId), AuthPolicies.ViewFtpCredential);

                    if (!canViewFtpCredential.Succeeded)
                    {
                        logger.LogWarning("User {UserId} denied access to view FTP credentials for game server {GameServerId}",
                            User.XtremeIdiotsId(), gameServerDto.GameServerId);

                        var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                            .Enrich(User)
                            .Enrich(gameServerDto);
                        unauthorizedTelemetry.Properties.TryAdd("Controller", "Credentials");
                        unauthorizedTelemetry.Properties.TryAdd("Action", "ViewFtpCredential");
                        unauthorizedTelemetry.Properties.TryAdd("Resource", "FtpCredential");
                        unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerDto.GameType},GameServerId:{gameServerDto.GameServerId}");
                        telemetryClient.TrackEvent(unauthorizedTelemetry);

                        gameServerDto.ClearFtpCredentials();
                    }

                    var canViewRconCredential = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.GameServerId), AuthPolicies.ViewRconCredential);

                    if (!canViewRconCredential.Succeeded)
                    {
                        logger.LogWarning("User {UserId} denied access to view RCON credentials for game server {GameServerId}",
                            User.XtremeIdiotsId(), gameServerDto.GameServerId);

                        var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                            .Enrich(User)
                            .Enrich(gameServerDto);
                        unauthorizedTelemetry.Properties.TryAdd("Controller", "Credentials");
                        unauthorizedTelemetry.Properties.TryAdd("Action", "ViewRconCredential");
                        unauthorizedTelemetry.Properties.TryAdd("Resource", "RconCredential");
                        unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerDto.GameType},GameServerId:{gameServerDto.GameServerId}");
                        telemetryClient.TrackEvent(unauthorizedTelemetry);

                        gameServerDto.ClearRconCredentials();
                    }
                }

                var eventTelemetry = new EventTelemetry("CredentialsViewed")
                    .Enrich(User);
                eventTelemetry.Properties.TryAdd("GameServerCount", gameServersList.Count.ToString());
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully viewed credentials for {GameServerCount} game servers",
                    User.XtremeIdiotsId(), gameServersList.Count);

                return View(gameServersList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading credentials view for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }
    }
}