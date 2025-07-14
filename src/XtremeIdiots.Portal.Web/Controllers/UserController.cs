using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing user accounts, profiles, and claims
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessUsers)]
    public class UserController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<UserController> logger;
        private readonly UserManager<IdentityUser> userManager;

        public UserController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<UserController> logger,
            UserManager<IdentityUser> userManager)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <summary>
        /// Displays the main users management page
        /// </summary>
        /// <returns>The users index view</returns>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                logger.LogInformation("User {UserId} accessing users index page", User.XtremeIdiotsId());
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing users index page");

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the permissions management page
        /// </summary>
        /// <returns>The permissions view</returns>
        [HttpGet]
        public IActionResult Permissions()
        {
            try
            {
                logger.LogInformation("User {UserId} accessing permissions page", User.XtremeIdiotsId());
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing permissions page");

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the user profile management page for a specific user
        /// </summary>
        /// <param name="id">The user profile ID to manage</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The manage profile view with user data and available game servers</returns>
        /// <exception cref="KeyNotFoundException">Thrown when user profile is not found</exception>
        [HttpGet]
        public async Task<IActionResult> ManageProfile(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to manage profile for user {ProfileId}",
                    User.XtremeIdiotsId(), id);

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin };
                var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

                var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
                    gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition, cancellationToken);

                var userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfile(id, cancellationToken);

                if (userProfileDtoApiResponse.IsNotFound)
                {
                    logger.LogWarning("User profile {ProfileId} not found when managing profile", id);
                    return NotFound();
                }

                if (gameServersApiResponse.Result?.Data?.Items == null || userProfileDtoApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Invalid API response when managing profile {ProfileId}", id);
                    return BadRequest();
                }

                ViewData["GameServers"] = gameServersApiResponse.Result.Data.Items;
                ViewData["GameServersSelect"] = new SelectList(gameServersApiResponse.Result.Data.Items, "GameServerId", "Title");

                logger.LogInformation("User {UserId} successfully accessed manage profile for user {ProfileId}",
                    User.XtremeIdiotsId(), id);

                return View(userProfileDtoApiResponse.Result.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error managing profile for user {ProfileId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ProfileId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// AJAX endpoint for retrieving users data for DataTables
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with user data for DataTables</returns>
        [HttpPost]
        public async Task<IActionResult> GetUsersAjax(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} requesting users AJAX data", User.XtremeIdiotsId());

                var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync(cancellationToken);

                var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

                if (model == null)
                {
                    logger.LogWarning("Invalid request body for users AJAX endpoint");
                    return BadRequest();
                }

                var userProfileResponseDto = await repositoryApiClient.UserProfiles.V1.GetUserProfiles(
                    model.Search?.Value, model.Start, model.Length, UserProfilesOrder.DisplayNameAsc, cancellationToken);

                if (userProfileResponseDto.Result?.Data == null)
                {
                    logger.LogWarning("Invalid API response for users AJAX endpoint");
                    return BadRequest();
                }

                logger.LogInformation("User {UserId} successfully retrieved {Count} users via AJAX",
                    User.XtremeIdiotsId(), userProfileResponseDto.Result.Data.Items?.Count() ?? 0);

                return Json(new
                {
                    model.Draw,
                    recordsTotal = userProfileResponseDto.Result.Data.TotalCount,
                    recordsFiltered = userProfileResponseDto.Result.Data.FilteredCount,
                    data = userProfileResponseDto.Result.Data.Items
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving users via AJAX");

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                return StatusCode(500, "An error occurred while retrieving users data");
            }
        }

        /// <summary>
        /// Forces a user to log out by updating their security stamp
        /// </summary>
        /// <param name="id">The user ID to force logout</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to Index with success/warning message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogUserOut(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to force logout user {TargetUserId}",
                    User.XtremeIdiotsId(), id);

                if (string.IsNullOrWhiteSpace(id))
                {
                    logger.LogWarning("Empty user ID provided for force logout");
                    return RedirectToAction(nameof(Index));
                }

                var user = await userManager.FindByIdAsync(id);

                if (user == null)
                {
                    logger.LogWarning("Could not find user with ID '{UserId}' for force logout", id);
                    this.AddAlertWarning($"Could not find user with XtremeIdiots ID '{id}', or there is no user logged in with that XtremeIdiots ID");
                    return RedirectToAction(nameof(Index));
                }

                await userManager.UpdateSecurityStampAsync(user);

                this.AddAlertSuccess($"User {user.UserName} has been force logged out (this may take up to 15 minutes)");
                logger.LogInformation("User {UserId} successfully force logged out user {TargetUser}",
                    User.XtremeIdiotsId(), user.UserName);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error forcing logout for user {TargetUserId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("TargetUserId", id);
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while forcing the user logout. Please try again.");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Creates a new user claim for the specified user profile
        /// </summary>
        /// <param name="id">The user profile ID to add the claim to</param>
        /// <param name="claimType">The type of claim to create</param>
        /// <param name="claimValue">The value of the claim (typically a game server ID)</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to ManageProfile with success/error message</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create user claims</exception>
        /// <exception cref="KeyNotFoundException">Thrown when user profile or game server is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserClaim(Guid id, string claimType, string claimValue, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to create claim {ClaimType}:{ClaimValue} for user profile {ProfileId}",
                    User.XtremeIdiotsId(), claimType, claimValue, id);

                var userProfileResponseDto = await repositoryApiClient.UserProfiles.V1.GetUserProfile(id, cancellationToken);

                if (userProfileResponseDto.IsNotFound)
                {
                    logger.LogWarning("User profile {ProfileId} not found when creating user claim", id);
                    return NotFound();
                }

                if (userProfileResponseDto.Result?.Data == null)
                {
                    logger.LogWarning("User profile data is null for {ProfileId}", id);
                    return BadRequest();
                }

                var userProfileData = userProfileResponseDto.Result.Data;
                var user = !string.IsNullOrEmpty(userProfileData.XtremeIdiotsForumId)
                    ? await userManager.FindByIdAsync(userProfileData.XtremeIdiotsForumId)
                    : null;

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(Guid.Parse(claimValue), cancellationToken);

                if (gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {GameServerId} not found when creating user claim", claimValue);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canCreateUserClaim = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.CreateUserClaim);

                if (!canCreateUserClaim.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create user claim for profile {ProfileId} and game type {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "User");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "CreateUserClaim");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "UserClaim");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ProfileId:{id},GameType:{gameServerData.GameType},ClaimType:{claimType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                if (!userProfileData.UserProfileClaims.Any(claim => claim.ClaimType == claimType && claim.ClaimValue == claimValue))
                {
                    var createUserProfileClaimDto = new CreateUserProfileClaimDto(userProfileData.UserProfileId, claimType, claimValue, false);

                    await repositoryApiClient.UserProfiles.V1.CreateUserProfileClaim(
                        userProfileData.UserProfileId, new List<CreateUserProfileClaimDto> { createUserProfileClaimDto }, cancellationToken);

                    this.AddAlertSuccess($"The {claimType} claim has been added to {user?.UserName ?? userProfileData.DisplayName}");
                    logger.LogInformation("User {UserId} successfully added claim {ClaimType}:{ClaimValue} to user profile {ProfileId}",
                        User.XtremeIdiotsId(), claimType, claimValue, id);

                    var eventTelemetry = new EventTelemetry("UserClaimCreated")
                        .Enrich(User);
                    eventTelemetry.Properties.TryAdd("ProfileId", id.ToString());
                    eventTelemetry.Properties.TryAdd("ClaimType", claimType);
                    eventTelemetry.Properties.TryAdd("ClaimValue", claimValue);
                    eventTelemetry.Properties.TryAdd("GameType", gameServerData.GameType.ToString());
                    telemetryClient.TrackEvent(eventTelemetry);
                }
                else
                {
                    this.AddAlertSuccess($"Nothing to do - {user?.UserName ?? userProfileData.DisplayName} already has the {claimType} claim");
                    logger.LogInformation("User {UserId} attempted to add duplicate claim {ClaimType}:{ClaimValue} to user profile {ProfileId}",
                        User.XtremeIdiotsId(), claimType, claimValue, id);
                }

                return RedirectToAction(nameof(ManageProfile), new { id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating user claim for profile {ProfileId}, claim {ClaimType}:{ClaimValue}",
                    id, claimType, claimValue);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ProfileId", id.ToString());
                errorTelemetry.Properties.TryAdd("ClaimType", claimType);
                errorTelemetry.Properties.TryAdd("ClaimValue", claimValue);
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while creating the user claim. Please try again.");
                return RedirectToAction(nameof(ManageProfile), new { id });
            }
        }

        /// <summary>
        /// Removes a user claim from the specified user profile
        /// </summary>
        /// <param name="id">The user profile ID to remove the claim from</param>
        /// <param name="claimId">The specific claim ID to remove</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to ManageProfile with success/error message</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete user claims</exception>
        /// <exception cref="KeyNotFoundException">Thrown when user profile or claim is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserClaim(Guid id, Guid claimId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to remove claim {ClaimId} from user profile {ProfileId}",
                    User.XtremeIdiotsId(), claimId, id);

                var userProfileResponseDto = await repositoryApiClient.UserProfiles.V1.GetUserProfile(id, cancellationToken);

                if (userProfileResponseDto.IsNotFound)
                {
                    logger.LogWarning("User profile {ProfileId} not found when removing user claim", id);
                    return NotFound();
                }

                if (userProfileResponseDto.Result?.Data == null)
                {
                    logger.LogWarning("User profile data is null for {ProfileId}", id);
                    return BadRequest();
                }

                var userProfileData = userProfileResponseDto.Result.Data;
                var claim = userProfileData.UserProfileClaims.SingleOrDefault(c => c.UserProfileClaimId == claimId);

                if (claim == null)
                {
                    logger.LogWarning("Claim {ClaimId} not found for user profile {ProfileId}", claimId, id);
                    return NotFound();
                }

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(Guid.Parse(claim.ClaimValue), cancellationToken);

                // Allow for legacy claims to be deleted
                var canDeleteUserClaim = false;
                if (gameServerApiResponse.IsNotFound)
                {
                    logger.LogInformation("Legacy claim detected for user profile {ProfileId}, allowing deletion", id);
                    canDeleteUserClaim = true;
                }
                else
                {
                    if (gameServerApiResponse.Result?.Data != null)
                    {
                        var authorizationResult = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.DeleteUserClaim);
                        canDeleteUserClaim = authorizationResult.Succeeded;
                    }
                }

                if (!canDeleteUserClaim)
                {
                    logger.LogWarning("User {UserId} denied access to delete user claim {ClaimId} for profile {ProfileId}",
                        User.XtremeIdiotsId(), claimId, id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "User");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "RemoveUserClaim");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "UserClaim");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ProfileId:{id},ClaimId:{claimId},ClaimType:{claim.ClaimType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                await repositoryApiClient.UserProfiles.V1.DeleteUserProfileClaim(id, claimId, cancellationToken);

                var user = !string.IsNullOrEmpty(userProfileData.XtremeIdiotsForumId)
                    ? await userManager.FindByIdAsync(userProfileData.XtremeIdiotsForumId)
                    : null;
                if (user != null)
                    await userManager.UpdateSecurityStampAsync(user);

                this.AddAlertSuccess($"User {userProfileData.DisplayName}'s claim has been removed (this may take up to 15 minutes)");
                logger.LogInformation("User {UserId} successfully removed claim {ClaimId} from user profile {ProfileId}",
                    User.XtremeIdiotsId(), claimId, id);

                var eventTelemetry = new EventTelemetry("UserClaimRemoved")
                    .Enrich(User);
                eventTelemetry.Properties.TryAdd("ProfileId", id.ToString());
                eventTelemetry.Properties.TryAdd("ClaimId", claimId.ToString());
                eventTelemetry.Properties.TryAdd("ClaimType", claim.ClaimType);
                eventTelemetry.Properties.TryAdd("ClaimValue", claim.ClaimValue);
                telemetryClient.TrackEvent(eventTelemetry);

                return RedirectToAction(nameof(ManageProfile), new { id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing user claim {ClaimId} from profile {ProfileId}", claimId, id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ProfileId", id.ToString());
                errorTelemetry.Properties.TryAdd("ClaimId", claimId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while removing the user claim. Please try again.");
                return RedirectToAction(nameof(ManageProfile), new { id });
            }
        }
    }
}