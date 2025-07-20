using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Controllers;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing game server credentials display and access .
/// Provides secure access to FTP and RCON credentials for authorized game server administrators.
/// </summary>
/// <remarks>
/// This controller handles credential viewing for Call of Duty game servers, applying granular 
/// authorization policies to ensure users only see credentials they're authorized to access.
/// Authorization is applied at both controller and individual credential levels.
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessCredentials)]
public class CredentialsController(
 IAuthorizationService authorizationService,
 IRepositoryApiClient repositoryApiClient,
 TelemetryClient telemetryClient,
 ILogger<CredentialsController> logger,
 IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
 private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
 private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

 /// <summary>
 /// Displays the credentials index page with filtered game servers based on user permissions.
 /// Shows FTP and RCON credentials for game servers the user is authorized to access.
 /// </summary>
 /// <param name="cancellationToken">Token to monitor for cancellation requests during the async operation</param>
 /// <returns>
 /// credentials index view containing game servers with credentials the user can access.
 /// Redirects to error page if game server retrieval fails.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionaccess credentials</exception>
 /// <exception cref="InvalidOperationException">Thrown when game server API call fails</exception>
 [HttpGet]
 public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.FtpCredentials, UserProfileClaimType.RconCredentials };
 var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

 Logger.LogInformation("User {UserId} querying game servers for credentials with {GameTypeCount} game types and {GameServerIdCount} specific servers",
 User.XtremeIdiotsId(), gameTypes?.Length ?? 0, gameServerIds?.Length ?? 0);

 var gameServersList = await GetAuthorizedGameServersAsync(gameTypes, gameServerIds, cancellationToken);
 if (gameServersList is null)
 {
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController).Replace("Controller", ""), new { id = 500 });
 }

 await ApplyCredentialAuthorizationAsync(gameServersList, cancellationToken);

 TrackSuccessTelemetry(nameof(Index), nameof(CredentialsController), new Dictionary<string, string>
 {
 { nameof(CredentialsController), nameof(CredentialsController) },
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
 /// Retrieves game servers that the user is authorized to view credentials for based on their claims.
 /// Filters servers by game types and specific server IDs that the user has permission to access.
 /// </summary>
 /// <param name="gameTypes">Array of game types the user has administrative access to</param>
 /// <param name="gameServerIds">Array of specific game server IDs the user has access to</param>
 /// <param name="cancellationToken">Token to monitor for cancellation requests during the async operation</param>
 /// <returns>
 /// List of game servers the user can access for credential viewing, or null if the API call fails.
 /// Returns empty list if user has no accessible servers.
 /// </returns>
 private async Task<List<GameServerDto>?> GetAuthorizedGameServersAsync(GameType[]? gameTypes, Guid[]? gameServerIds, CancellationToken cancellationToken)
 {
 var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
 gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition, cancellationToken);

 if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items is null)
 {
 Logger.LogWarning("Failed to retrieve game servers for credentials view for user {UserId} - API response status: {IsSuccess}",
 User.XtremeIdiotsId(), gameServersApiResponse.IsSuccess);

 TelemetryClient.TrackEvent("CredentialsApiFailure", new Dictionary<string, string>
 {
 { "ApiSuccess", gameServersApiResponse.IsSuccess.ToString() },
 { nameof(CredentialsController), nameof(CredentialsController) },
 { "Action", nameof(GetAuthorizedGameServersAsync) },
 { "UserId", User.XtremeIdiotsId()?.ToString() ?? "Unknown" }
 });

 return null;
 }

 return gameServersApiResponse.Result.Data.Items.ToList();
 }

 /// <summary>
 /// Applies credential-specific authorization to each game server, clearing credentials for unauthorized access.
 /// Performs fine-grained authorization checks for FTP and RCON credentials separately for each server.
 /// </summary>
 /// <param name="gameServersList">List of game servers to apply credential authorization filtering to</param>
 /// <param name="cancellationToken">Token to monitor for cancellation requests during the async operation</param>
 /// <remarks>
 /// This method modifies the game server DTOs in-place, clearing FTP or RCON credentials
 /// from servers where the user lacks specific credential access permissions.
 /// Unauthorized access attempts are tracked for security monitoring.
 /// </remarks>
 private async Task ApplyCredentialAuthorizationAsync(List<GameServerDto> gameServersList, CancellationToken cancellationToken)
 {
 foreach (var gameServerDto in gameServersList)
 {
 var ftpResource = new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.GameServerId);
 var canViewFtpCredential = await authorizationService.AuthorizeAsync(User, ftpResource, AuthPolicies.ViewFtpCredential);

 if (!canViewFtpCredential.Succeeded)
 {
 TrackUnauthorizedAccessAttempt(nameof(AuthPolicies.ViewFtpCredential), "FtpCredential",
 $"GameType:{gameServerDto.GameType},GameServerId:{gameServerDto.GameServerId}", gameServerDto);
 gameServerDto.ClearFtpCredentials();
 }

 var canViewRconCredential = await authorizationService.AuthorizeAsync(User, ftpResource, AuthPolicies.ViewRconCredential);

 if (!canViewRconCredential.Succeeded)
 {
 TrackUnauthorizedAccessAttempt(nameof(AuthPolicies.ViewRconCredential), "RconCredential",
 $"GameType:{gameServerDto.GameType},GameServerId:{gameServerDto.GameServerId}", gameServerDto);
 gameServerDto.ClearRconCredentials();
 }
 }
 }
}