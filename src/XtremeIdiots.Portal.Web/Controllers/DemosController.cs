using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Demos;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller responsible for managing demo files including upload, download, and deletion functionality.
    /// Provides both web UI and client API endpoints for demo management.
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessDemos)]
    public class DemosController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly ILogger<DemosController> logger;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IDemoManager demosForumsClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly UserManager<IdentityUser> userManager;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the DemosController with required dependencies.
        /// </summary>
        /// <param name="logger">Logger instance for structured logging</param>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="userManager">Manager for identity user operations</param>
        /// <param name="signInManager">Manager for sign-in operations</param>
        /// <param name="demosForumsClient">Client for demo forum integration</param>
        /// <param name="repositoryApiClient">Client for repository API operations</param>
        /// <param name="telemetryClient">Client for application insights telemetry</param>
        public DemosController(
            ILogger<DemosController> logger,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IDemoManager demosForumsClient,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.demosForumsClient = demosForumsClient ?? throw new ArgumentNullException(nameof(demosForumsClient));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <summary>
        /// Displays the demo client configuration page with user authentication key.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Demo client configuration view</returns>
        [HttpGet]
        public async Task<IActionResult> DemoClient(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogInformation("User {UserId} accessing demo client configuration page", userId);

                var userProfileApiResponse = await this.repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(userId);

                if (!userProfileApiResponse.IsNotFound && userProfileApiResponse.Result?.Data != null)
                {
                    ViewData["ClientAuthKey"] = userProfileApiResponse.Result.Data.DemoAuthKey;
                }

                var demoManagerClientDto = await this.demosForumsClient.GetDemoManagerClient();

                var demoClientTelemetry = new EventTelemetry("DemoClientPageViewed")
                    .Enrich(User);
                demoClientTelemetry.Properties.TryAdd("HasAuthKey", (!userProfileApiResponse.IsNotFound && userProfileApiResponse.Result?.Data != null).ToString());
                this.telemetryClient.TrackEvent(demoClientTelemetry);

                return View(demoManagerClientDto);
            }
            catch (Exception ex)
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogError(ex, "Error loading demo client configuration for user {UserId}", userId);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Enrich(User);
                exceptionTelemetry.Properties.TryAdd("ActionType", "DemoClientAccess");
                this.telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Regenerates the demo authentication key for the current user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Redirect to demo client page with success message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateAuthKey(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogInformation("User {UserId} attempting to regenerate demo auth key", userId);

                var userProfileApiResponse = await this.repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(userId);

                if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data == null)
                {
                    this.logger.LogWarning("User profile {UserId} not found when regenerating demo auth key", userId);
                    return NotFound();
                }

                var editUserProfileDto = new EditUserProfileDto(userProfileApiResponse.Result.Data.UserProfileId)
                {
                    DemoAuthKey = Guid.NewGuid().ToString()
                };

                await this.repositoryApiClient.UserProfiles.V1.UpdateUserProfile(editUserProfileDto);

                this.logger.LogInformation("User {UserId} successfully regenerated demo auth key", userId);
                this.AddAlertSuccess("Your demo auth key has been regenerated, you will need to reconfigure your client desktop application");

                var authKeyTelemetry = new EventTelemetry("DemoAuthKeyRegenerated")
                    .Enrich(User);
                authKeyTelemetry.Properties.TryAdd("UserProfileId", userProfileApiResponse.Result.Data.UserProfileId.ToString());
                this.telemetryClient.TrackEvent(authKeyTelemetry);

                return RedirectToAction(nameof(DemoClient));
            }
            catch (Exception ex)
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogError(ex, "Error regenerating demo auth key for user {UserId}", userId);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Enrich(User);
                exceptionTelemetry.Properties.TryAdd("ActionType", "RegenerateAuthKey");
                this.telemetryClient.TrackException(exceptionTelemetry);

                this.AddAlertDanger("An error occurred while regenerating your demo auth key. Please try again.");
                return RedirectToAction(nameof(DemoClient));
            }
        }

        /// <summary>
        /// Displays the main demos index page.
        /// </summary>
        /// <returns>Demos index view</returns>
        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.XtremeIdiotsId();
            this.logger.LogInformation("User {UserId} accessing demos index page", userId);

            var indexTelemetry = new EventTelemetry("DemosIndexViewed")
                .Enrich(User);
            this.telemetryClient.TrackEvent(indexTelemetry);

            return View();
        }

        /// <summary>
        /// Displays the demos index page filtered by a specific game type.
        /// </summary>
        /// <param name="id">The game type to filter by</param>
        /// <returns>Filtered demos index view</returns>
        [HttpGet]
        public IActionResult GameIndex(GameType? id)
        {
            var userId = User.XtremeIdiotsId();
            this.logger.LogInformation("User {UserId} accessing game-specific demos index for game type {GameType}", userId, id);

            ViewData["GameType"] = id;

            var gameIndexTelemetry = new EventTelemetry("GameDemosIndexViewed")
                .Enrich(User);
            gameIndexTelemetry.Properties.TryAdd("GameType", id?.ToString() ?? "null");
            this.telemetryClient.TrackEvent(gameIndexTelemetry);

            return View(nameof(Index));
        }

        /// <summary>
        /// Provides AJAX endpoint for DataTables to load demo data with filtering and pagination.
        /// Supports game type filtering and role-based access control for demo management.
        /// </summary>
        /// <param name="id">Optional game type filter to restrict results to a specific game</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>JSON response with demo data formatted for DataTables consumption</returns>
        /// <exception cref="BadRequestException">Thrown when the request model is invalid</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks appropriate permissions</exception>
        [HttpPost]
        public async Task<IActionResult> GetDemoListAjax(GameType? id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogInformation("User {UserId} requesting demo list via AJAX for game type {GameType}", userId, id);

                var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

                if (model == null)
                {
                    this.logger.LogWarning("User {UserId} provided invalid request model for demo list AJAX", userId);
                    return BadRequest();
                }

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.Moderator };
                var gameTypes = User.ClaimedGameTypes(requiredClaims);

                string? filterUserId = null;
                GameType[]? filterGameTypes;
                if (id != null)
                {
                    filterGameTypes = new[] { (GameType)id };
                    // If the user has the required claims do not filter by user id
                    filterUserId = gameTypes.Contains((GameType)id) ? null : userId;
                }
                else
                {
                    filterGameTypes = gameTypes.ToArray();

                    // If the user has any required claims for games do not filter by user id
                    if (!gameTypes.Any()) filterUserId = userId;
                }

                var order = DemoOrder.CreatedDesc;
                if (model.Order != null)
                {
                    var orderColumn = model.Columns[model.Order.First().Column].Name;
                    var searchOrder = model.Order.First().Dir;

                    switch (orderColumn)
                    {
                        case "game":
                            order = searchOrder == "asc" ? DemoOrder.GameTypeAsc : DemoOrder.GameTypeDesc;
                            break;
                        case "name":
                            order = searchOrder == "asc" ? DemoOrder.TitleAsc : DemoOrder.TitleDesc;
                            break;
                        case "date":
                            order = searchOrder == "asc" ? DemoOrder.CreatedAsc : DemoOrder.CreatedDesc;
                            break;
                        case "uploadedBy":
                            order = searchOrder == "asc" ? DemoOrder.UploadedByAsc : DemoOrder.UploadedByDesc;
                            break;
                    }
                }

                var demosApiResponse = await this.repositoryApiClient.Demos.V1.GetDemos(filterGameTypes, filterUserId, model.Search?.Value, model.Start, model.Length, order);

                if (!demosApiResponse.IsSuccess || demosApiResponse.Result?.Data == null)
                {
                    this.logger.LogError("Failed to retrieve demos list for user {UserId}", userId);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var portalDemoEntries = new List<PortalDemoDto>();
                if (demosApiResponse.Result.Data.Items != null)
                {
                    foreach (var demoDto in demosApiResponse.Result.Data.Items)
                    {
                        var canDeletePortalDemo = await this.authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(demoDto.GameType, demoDto.UserProfileId), AuthPolicies.DeleteDemo);

                        var portalDemoDto = new PortalDemoDto(demoDto);

                        if (canDeletePortalDemo.Succeeded)
                            portalDemoDto.ShowDeleteLink = true;

                        portalDemoEntries.Add(portalDemoDto);
                    }
                }

                var demoListTelemetry = new EventTelemetry("DemoListLoaded")
                    .Enrich(User);
                demoListTelemetry.Properties.TryAdd("GameType", id?.ToString() ?? "All");
                demoListTelemetry.Properties.TryAdd("ResultCount", portalDemoEntries.Count.ToString());
                demoListTelemetry.Properties.TryAdd("TotalCount", demosApiResponse.Result.Data.TotalCount.ToString());
                this.telemetryClient.TrackEvent(demoListTelemetry);

                return Json(new
                {
                    model.Draw,
                    recordsTotal = demosApiResponse.Result.Data.TotalCount,
                    recordsFiltered = demosApiResponse.Result.Data.FilteredCount,
                    data = portalDemoEntries
                });
            }
            catch (Exception ex)
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogError(ex, "Error loading demo list for user {UserId} with game type {GameType}", userId, id);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("LoggedInAdminId", userId);
                exceptionTelemetry.Properties.TryAdd("GameType", id?.ToString() ?? "All");
                exceptionTelemetry.Properties.TryAdd("ActionType", "GetDemoListAjax");
                this.telemetryClient.TrackException(exceptionTelemetry);

                return RedirectToAction("Display", "Errors", new { id = 500 });
            }
        }

        /// <summary>
        /// Downloads a demo file by redirecting to its storage URI.
        /// </summary>
        /// <param name="id">The unique identifier of the demo</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Redirect to demo file URI or NotFound if demo doesn't exist</returns>
        [HttpGet]
        public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogInformation("User {UserId} attempting to download demo {DemoId}", userId, id);

                var demoApiResult = await this.repositoryApiClient.Demos.V1.GetDemo(id);

                if (demoApiResult.IsNotFound || demoApiResult.Result?.Data == null)
                {
                    this.logger.LogWarning("Demo {DemoId} not found when user {UserId} attempted download", id, userId);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Demos");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Download");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "Demo");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"DemoId:{id},Reason:NotFound");
                    this.telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return NotFound();
                }

                this.logger.LogInformation("User {UserId} successfully downloading demo {DemoId} ({DemoTitle})",
                    userId, id, demoApiResult.Result.Data.Title);

                var downloadTelemetry = new EventTelemetry("DemoDownloaded")
                    .Enrich(User)
                    .Enrich(demoApiResult.Result.Data);
                this.telemetryClient.TrackEvent(downloadTelemetry);

                return Redirect(demoApiResult.Result.Data.FileUri);
            }
            catch (Exception ex)
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogError(ex, "Error downloading demo {DemoId} for user {UserId}", id, userId);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Enrich(User);
                exceptionTelemetry.Properties.TryAdd("DemoId", id.ToString());
                exceptionTelemetry.Properties.TryAdd("ActionType", "Download");
                this.telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the delete confirmation page for a demo.
        /// </summary>
        /// <param name="id">The unique identifier of the demo</param>
        /// <param name="filterGame">Whether to return to game-filtered view after deletion</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Delete confirmation view or authorization error</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id, bool filterGame = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogInformation("User {UserId} attempting to access delete page for demo {DemoId}", userId, id);

                var demoApiResult = await this.repositoryApiClient.Demos.V1.GetDemo(id);

                if (demoApiResult.IsNotFound || demoApiResult.Result?.Data == null)
                {
                    this.logger.LogWarning("Demo {DemoId} not found when user {UserId} attempted to delete", id, userId);
                    return NotFound();
                }

                var authorizationResource = new Tuple<GameType, Guid>(demoApiResult.Result.Data.GameType, demoApiResult.Result.Data.UserProfileId);
                var canDeleteDemo = await this.authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.DeleteDemo);

                if (!canDeleteDemo.Succeeded)
                {
                    this.logger.LogWarning("User {UserId} denied access to delete demo {DemoId}", userId, id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(demoApiResult.Result.Data);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Demos");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "Demo");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{demoApiResult.Result.Data.GameType},Reason:Unauthorized");
                    this.telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                ViewData["FilterGame"] = filterGame;

                var deleteTelemetry = new EventTelemetry("DemoDeletePageViewed")
                    .Enrich(User)
                    .Enrich(demoApiResult.Result.Data);
                this.telemetryClient.TrackEvent(deleteTelemetry);

                return View(demoApiResult.Result);
            }
            catch (Exception ex)
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogError(ex, "Error accessing delete page for demo {DemoId} by user {UserId}", id, userId);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("LoggedInAdminId", userId);
                exceptionTelemetry.Properties.TryAdd("DemoId", id.ToString());
                exceptionTelemetry.Properties.TryAdd("ActionType", "DeleteGet");
                this.telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Processes the demo deletion after confirmation.
        /// </summary>
        /// <param name="id">The unique identifier of the demo</param>
        /// <param name="filterGame">Whether to return to game-filtered view after deletion</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Redirect to demos list with success message</returns>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, bool filterGame = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogInformation("User {UserId} attempting to delete demo {DemoId}", userId, id);

                var demoApiResult = await this.repositoryApiClient.Demos.V1.GetDemo(id);

                if (demoApiResult.IsNotFound || demoApiResult.Result?.Data == null)
                {
                    this.logger.LogWarning("Demo {DemoId} not found when user {UserId} attempted deletion", id, userId);
                    return NotFound();
                }

                var authorizationResource = new Tuple<GameType, Guid>(demoApiResult.Result.Data.GameType, demoApiResult.Result.Data.UserProfileId);
                var canDeleteDemo = await this.authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.DeleteDemo);

                if (!canDeleteDemo.Succeeded)
                {
                    this.logger.LogWarning("User {UserId} denied access to delete demo {DemoId}", userId, id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(demoApiResult.Result.Data);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Demos");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "DeleteConfirmed");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "Demo");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{demoApiResult.Result.Data.GameType},Reason:Unauthorized");
                    this.telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                await this.repositoryApiClient.Demos.V1.DeleteDemo(id);

                this.logger.LogInformation("User {UserId} successfully deleted demo {DemoId} ({DemoTitle}) under {GameType}",
                    userId, id, demoApiResult.Result.Data.Title, demoApiResult.Result.Data.GameType);

                this.AddAlertSuccess($"The demo {demoApiResult.Result.Data.Title} has been successfully deleted from {demoApiResult.Result.Data.GameType}");

                var deletedTelemetry = new EventTelemetry("DemoDeleted")
                    .Enrich(User)
                    .Enrich(demoApiResult.Result.Data);
                this.telemetryClient.TrackEvent(deletedTelemetry);

                if (filterGame)
                    return RedirectToAction(nameof(GameIndex), new { id = demoApiResult.Result.Data.GameType });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var userId = User.XtremeIdiotsId();
                this.logger.LogError(ex, "Error deleting demo {DemoId} for user {UserId}", id, userId);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("LoggedInAdminId", userId);
                exceptionTelemetry.Properties.TryAdd("DemoId", id.ToString());
                exceptionTelemetry.Properties.TryAdd("ActionType", "DeleteConfirmed");
                this.telemetryClient.TrackException(exceptionTelemetry);

                this.AddAlertDanger("An error occurred while deleting the demo. Please try again.");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Provides API endpoint for demo client applications to retrieve demo list.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>JSON array of demos or authentication error message</returns>
        [HttpGet]
        public async Task<IActionResult> ClientDemoList(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                {
                    this.logger.LogDebug("ClientDemoList - No auth key provided in request headers");
                    return Content("AuthError: No auth key provided in the request. This should be set in the client.");
                }

                var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(authKey))
                {
                    this.logger.LogDebug("ClientDemoList - Auth key header supplied but was empty");
                    return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
                }

                var userProfileApiResponse = await this.repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey);

                if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data == null)
                {
                    this.logger.LogWarning("ClientDemoList - Invalid auth key provided: {AuthKeyPrefix}", authKey.Substring(0, Math.Min(4, authKey.Length)));
                    return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");
                }

                var userIdFromProfile = userProfileApiResponse.Result.Data.XtremeIdiotsForumId;
                if (string.IsNullOrWhiteSpace(userIdFromProfile))
                {
                    this.logger.LogError("ClientDemoList - User profile missing XtremeIdiotsForumId for profile {UserProfileId}", userProfileApiResponse.Result.Data.UserProfileId);
                    return Content("AuthError: An internal auth error occurred processing your request - missing user ID.");
                }

                var user = await this.userManager.FindByIdAsync(userIdFromProfile);
                if (user == null)
                {
                    this.logger.LogWarning("ClientDemoList - User not found for ID {UserId}", userIdFromProfile);
                    return Content($"AuthError: An internal auth error occurred processing your request for userId: {userIdFromProfile}");
                }

                var claimsPrincipal = await this.signInManager.ClaimsFactory.CreateAsync(user);

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.Moderator };
                var gameTypes = claimsPrincipal.ClaimedGameTypes(requiredClaims);

                string? filterUserId = null;
                GameType[]? filterGameTypes = gameTypes.ToArray();
                if (!gameTypes.Any()) filterUserId = userIdFromProfile;

                var demosApiResponse = await this.repositoryApiClient.Demos.V1.GetDemos(filterGameTypes, filterUserId, null, 0, 500, DemoOrder.CreatedDesc);

                if (!demosApiResponse.IsSuccess || demosApiResponse.Result?.Data?.Items == null)
                {
                    this.logger.LogError("ClientDemoList - Failed to retrieve demos for user {UserId}", userIdFromProfile);
                    return Content("Error: Failed to retrieve demo list from server.");
                }

                var demos = demosApiResponse.Result.Data.Items.Select(demo => new
                {
                    demo.DemoId,
                    Version = demo.GameType.ToString(),
                    Name = demo.Title,
                    Date = demo.Created,
                    demo.Map,
                    demo.Mod,
                    GameType = demo.GameMode,
                    Server = demo.ServerName,
                    Size = demo.FileSize,
                    Identifier = demo.FileName,
                    demo.FileName
                }).ToList();

                this.logger.LogInformation("ClientDemoList - Successfully provided {DemoCount} demos to client for user {UserId}", demos.Count, userIdFromProfile);

                var clientListTelemetry = new EventTelemetry("ClientDemoListProvided");
                clientListTelemetry.Properties.TryAdd("LoggedInAdminId", userIdFromProfile);
                clientListTelemetry.Properties.TryAdd("DemoCount", demos.Count.ToString());
                this.telemetryClient.TrackEvent(clientListTelemetry);

                return Json(demos);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in ClientDemoList endpoint");

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("ActionType", "ClientDemoList");
                this.telemetryClient.TrackException(exceptionTelemetry);

                return Content("Error: An internal server error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Handles demo file uploads from client applications.
        /// </summary>
        /// <param name="file">The demo file to upload</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Success response or error message</returns>
        [HttpPost]
        public async Task<ActionResult> ClientUploadDemo(IFormFile file, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                {
                    this.logger.LogDebug("ClientUploadDemo - No auth key provided in request headers");
                    return Content("AuthError: No auth key provided in the request. This should be set in the client.");
                }

                var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(authKey))
                {
                    this.logger.LogDebug("ClientUploadDemo - Auth key header supplied was empty");
                    return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
                }

                var userProfileApiResponse = await this.repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey);

                if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data == null)
                {
                    this.logger.LogWarning("ClientUploadDemo - Invalid auth key provided: {AuthKeyPrefix}", authKey.Substring(0, Math.Min(4, authKey.Length)));
                    return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");
                }

                var userIdFromProfile = userProfileApiResponse.Result.Data.XtremeIdiotsForumId;
                if (string.IsNullOrWhiteSpace(userIdFromProfile))
                {
                    this.logger.LogError("ClientUploadDemo - User profile missing XtremeIdiotsForumId for profile {UserProfileId}", userProfileApiResponse.Result.Data.UserProfileId);
                    return Content("AuthError: An internal auth error occurred processing your request - missing user ID.");
                }

                var gameTypeHeader = Request.Headers["demo-manager-game-type"].ToString();
                if (!Enum.TryParse(gameTypeHeader, out GameType gameType))
                {
                    this.logger.LogWarning("ClientUploadDemo - Invalid or missing game type header: {GameTypeHeader}", gameTypeHeader);
                    return Content("Error: Invalid or missing game type. Please ensure your client is properly configured.");
                }

                if (file == null || file.Length == 0)
                {
                    this.logger.LogWarning("ClientUploadDemo - No file provided by user {UserId}", userIdFromProfile);
                    return Content("You must provide a file to be uploaded");
                }

                var whitelistedExtensions = new List<string> { ".dm_1", ".dm_6" };

                if (!whitelistedExtensions.Any(ext => file.FileName.EndsWith(ext)))
                {
                    this.logger.LogWarning("ClientUploadDemo - Invalid file extension {FileName} by user {UserId}", file.FileName, userIdFromProfile);
                    return Content("Invalid file type extension");
                }

                var filePath = Path.Join(Path.GetTempPath(), file.FileName);
                using (var stream = System.IO.File.Create(filePath))
                    await file.CopyToAsync(stream, cancellationToken);

                var demoDto = new CreateDemoDto(gameType, userProfileApiResponse.Result.Data.UserProfileId);

                var createDemoApiResponse = await this.repositoryApiClient.Demos.V1.CreateDemo(demoDto);
                if (createDemoApiResponse.IsSuccess && createDemoApiResponse.Result?.Data != null)
                {
                    await this.repositoryApiClient.Demos.V1.SetDemoFile(createDemoApiResponse.Result.Data.DemoId, file.FileName, filePath);
                }

                this.logger.LogInformation("User {UserId} successfully uploaded demo {FileName} for game type {GameType}",
                    userIdFromProfile, file.FileName, gameType);

                var clientUploadTelemetry = new EventTelemetry("ClientDemoUploaded");
                clientUploadTelemetry.Properties.TryAdd("LoggedInAdminId", userIdFromProfile);
                clientUploadTelemetry.Properties.TryAdd("FileName", file.FileName);
                clientUploadTelemetry.Properties.TryAdd("GameType", gameType.ToString());
                clientUploadTelemetry.Properties.TryAdd("FileSize", file.Length.ToString());
                this.telemetryClient.TrackEvent(clientUploadTelemetry);

                return Ok();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in ClientUploadDemo endpoint");

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("ActionType", "ClientUploadDemo");
                if (file != null)
                {
                    exceptionTelemetry.Properties.TryAdd("FileName", file.FileName);
                    exceptionTelemetry.Properties.TryAdd("FileSize", file.Length.ToString());
                }
                this.telemetryClient.TrackException(exceptionTelemetry);

                return Content("Error: An internal server error occurred while uploading your demo.");
            }
        }

        /// <summary>
        /// Provides demo download endpoint for client applications.
        /// </summary>
        /// <param name="id">The unique identifier of the demo</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Redirect to demo file URI or error message</returns>
        [HttpGet]
        public async Task<IActionResult> ClientDownload(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                {
                    this.logger.LogDebug("ClientDownload - No auth key provided in request headers");
                    return Content("AuthError: No auth key provided in the request. This should be set in the client.");
                }

                var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(authKey))
                {
                    this.logger.LogDebug("ClientDownload - Auth key header supplied but was empty");
                    return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
                }

                var userProfileApiResponse = await this.repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey);

                if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data == null)
                {
                    this.logger.LogWarning("ClientDownload - Invalid auth key provided: {AuthKeyPrefix}", authKey.Substring(0, Math.Min(4, authKey.Length)));
                    return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");
                }

                var userIdFromProfile = userProfileApiResponse.Result.Data.XtremeIdiotsForumId;
                if (string.IsNullOrWhiteSpace(userIdFromProfile))
                {
                    this.logger.LogError("ClientDownload - User profile missing XtremeIdiotsForumId for profile {UserProfileId}", userProfileApiResponse.Result.Data.UserProfileId);
                    return Content("AuthError: An internal auth error occurred processing your request - missing user ID.");
                }

                var demoApiResponse = await this.repositoryApiClient.Demos.V1.GetDemo(id);

                if (demoApiResponse.IsNotFound || demoApiResponse.Result?.Data == null)
                {
                    this.logger.LogWarning("ClientDownload - Demo {DemoId} not found for user {UserId}", id, userIdFromProfile);
                    return NotFound();
                }

                this.logger.LogInformation("User {UserId} successfully downloading demo {DemoId} via client", userIdFromProfile, id);

                var clientDownloadTelemetry = new EventTelemetry("ClientDemoDownloaded")
                    .Enrich(demoApiResponse.Result.Data);
                clientDownloadTelemetry.Properties.TryAdd("LoggedInAdminId", userIdFromProfile);
                this.telemetryClient.TrackEvent(clientDownloadTelemetry);

                return Redirect(demoApiResponse.Result.Data.FileUri);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in ClientDownload endpoint for demo {DemoId}", id);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("DemoId", id.ToString());
                exceptionTelemetry.Properties.TryAdd("ActionType", "ClientDownload");
                this.telemetryClient.TrackException(exceptionTelemetry);

                return Content("Error: An internal server error occurred while downloading the demo.");
            }
        }

        public class PortalDemoDto
        {
            public PortalDemoDto(DemoDto demo)
            {
                DemoId = demo.DemoId;
                Game = demo.GameType.ToString();
                Name = demo.Title;
                FileName = demo.FileName;
                Date = demo.Created;
                Map = demo.Map;
                Mod = demo.Mod;
                GameType = demo.GameMode;
                Server = demo.ServerName;
                Size = demo.FileSize;
                UserId = demo.UserProfile?.XtremeIdiotsForumId ?? "21145";
                UploadedBy = demo.UserProfile?.DisplayName ?? "Admin";
            }

            public Guid DemoId { get; set; }
            public string Game { get; set; }
            public string Name { get; set; }
            public string FileName { get; set; }
            public DateTime? Date { get; set; }
            public string Map { get; set; }
            public string Mod { get; set; }
            public string GameType { get; set; }
            public string Server { get; set; }
            public long Size { get; set; }
            public string UserId { get; set; }
            public string UploadedBy { get; set; }

            public bool ShowDeleteLink { get; set; }
        }
    }
}