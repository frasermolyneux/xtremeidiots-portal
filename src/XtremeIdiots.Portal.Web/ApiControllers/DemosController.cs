using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Demos;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ApiControllers
{
    /// <summary>
    /// API controller for demo-related endpoints that provide JSON data
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessDemos)]
    [Route("Demos")]
    public class DemosController : BaseApiController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly UserManager<IdentityUser> userManager;

        /// <summary>
        /// Initializes a new instance of the DemosController with required dependencies.
        /// </summary>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="userManager">Manager for identity user operations</param>
        /// <param name="signInManager">Manager for sign-in operations</param>
        /// <param name="repositoryApiClient">Client for repository API operations</param>
        /// <param name="telemetryClient">Client for application insights telemetry</param>
        /// <param name="logger">Logger instance for structured logging</param>
        /// <param name="configuration">Configuration service for app settings</param>
        public DemosController(
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<DemosController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Returns demos data as JSON for DataTable with optional game type filter
        /// </summary>
        /// <param name="id">Optional game type to filter demos by</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>JSON response with demo data formatted for DataTables consumption</returns>
        [HttpPost("GetDemoListAjax")]
        public async Task<IActionResult> GetDemoListAjax(GameType? id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync(cancellationToken);

                var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

                if (model is null)
                {
                    Logger.LogWarning("Invalid request model for demo list AJAX from user {UserId}", User.XtremeIdiotsId());
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
                    filterUserId = gameTypes.Contains((GameType)id) ? null : User.XtremeIdiotsId();
                }
                else
                {
                    filterGameTypes = gameTypes.ToArray();

                    // If the user has any required claims for games do not filter by user id
                    if (!gameTypes.Any()) filterUserId = User.XtremeIdiotsId();
                }

                var order = GetDemoOrderFromDataTable(model);

                var demosApiResponse = await repositoryApiClient.Demos.V1.GetDemos(filterGameTypes, filterUserId, model.Search?.Value, model.Start, model.Length, order, cancellationToken);

                if (!demosApiResponse.IsSuccess || demosApiResponse.Result?.Data is null)
                {
                    Logger.LogError("Failed to retrieve demos list for user {UserId}", User.XtremeIdiotsId());
                    return StatusCode(500, "Failed to retrieve demos data");
                }

                var portalDemoEntries = new List<PortalDemoDto>();
                if (demosApiResponse.Result.Data.Items != null)
                {
                    foreach (var demoDto in demosApiResponse.Result.Data.Items)
                    {
                        var canDeletePortalDemo = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(demoDto.GameType, demoDto.UserProfileId), AuthPolicies.DeleteDemo);

                        var portalDemoDto = new PortalDemoDto(demoDto);

                        if (canDeletePortalDemo.Succeeded)
                            portalDemoDto.ShowDeleteLink = true;

                        portalDemoEntries.Add(portalDemoDto);
                    }
                }

                TrackSuccessTelemetry("DemoListLoaded", "GetDemoListAjax", new Dictionary<string, string>
                {
                    { "GameType", id?.ToString() ?? "All" },
                    { "ResultCount", portalDemoEntries.Count.ToString() },
                    { "TotalCount", demosApiResponse.Result.Data.TotalCount.ToString() }
                });

                return Ok(new
                {
                    model.Draw,
                    recordsTotal = demosApiResponse.Result.Data.TotalCount,
                    recordsFiltered = demosApiResponse.Result.Data.FilteredCount,
                    data = portalDemoEntries
                });
            }, "GetDemoListAjax");
        }

        /// <summary>
        /// Provides API endpoint for demo client applications to retrieve demo list.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>JSON array of demos or authentication error message</returns>
        [HttpGet("ClientDemoList")]
        public async Task<IActionResult> ClientDemoList(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                {
                    Logger.LogDebug("ClientDemoList - No auth key provided in request headers");
                    return Content("AuthError: No auth key provided in the request. This should be set in the client.");
                }

                var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(authKey))
                {
                    Logger.LogDebug("ClientDemoList - Auth key header supplied but was empty");
                    return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
                }

                var userProfileApiResponse = await this.repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey);

                if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data is null)
                {
                    Logger.LogWarning("ClientDemoList - Invalid auth key provided: {AuthKeyPrefix}", authKey.Substring(0, Math.Min(4, authKey.Length)));
                    return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");
                }

                var userIdFromProfile = userProfileApiResponse.Result.Data.XtremeIdiotsForumId;

                if (string.IsNullOrWhiteSpace(userIdFromProfile))
                {
                    Logger.LogError("ClientDemoList - User profile missing XtremeIdiotsForumId for profile {UserProfileId}", userProfileApiResponse.Result.Data.UserProfileId);
                    return Content("AuthError: An internal auth error occurred processing your request - missing user ID.");
                }

                var user = await this.userManager.FindByIdAsync(userIdFromProfile);
                if (user is null)
                {
                    Logger.LogWarning("ClientDemoList - User not found for ID {UserId}", userIdFromProfile);
                    return Content($"AuthError: An internal auth error occurred processing your request for userId: {userIdFromProfile}");
                }

                var claimsPrincipal = await this.signInManager.ClaimsFactory.CreateAsync(user);

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.Moderator };
                var gameTypes = claimsPrincipal.ClaimedGameTypes(requiredClaims);

                string? filterUserId = null;
                GameType[]? filterGameTypes = gameTypes.ToArray();
                if (!gameTypes.Any()) filterUserId = userIdFromProfile;

                var demosApiResponse = await this.repositoryApiClient.Demos.V1.GetDemos(filterGameTypes, filterUserId, null, 0, 500, DemoOrder.CreatedDesc);

                if (!demosApiResponse.IsSuccess || demosApiResponse.Result?.Data?.Items is null)
                {
                    Logger.LogError("ClientDemoList - Failed to retrieve demos for user {UserId}", userIdFromProfile);
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

                Logger.LogInformation("ClientDemoList - Successfully provided {DemoCount} demos to client for user {UserId}", demos.Count, userIdFromProfile);

                var clientListTelemetry = new EventTelemetry("ClientDemoListProvided");
                clientListTelemetry.Properties.TryAdd("LoggedInAdminId", userIdFromProfile);
                clientListTelemetry.Properties.TryAdd("DemoCount", demos.Count.ToString());
                TelemetryClient.TrackEvent(clientListTelemetry);

                return Ok(demos);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in ClientDemoList endpoint");

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("ActionType", "ClientDemoList");
                TelemetryClient.TrackException(exceptionTelemetry);

                return Content("Error: An internal server error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Helper method to determine the correct DemoOrder from DataTable model
        /// </summary>
        /// <param name="model">The DataTable AJAX model containing order information</param>
        /// <returns>The appropriate DemoOrder enum value</returns>
        private static DemoOrder GetDemoOrderFromDataTable(DataTableAjaxPostModel model)
        {
            var order = DemoOrder.CreatedDesc;

            if (model.Order != null && model.Order.Any())
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                order = orderColumn switch
                {
                    "game" => searchOrder == "asc" ? DemoOrder.GameTypeAsc : DemoOrder.GameTypeDesc,
                    "name" => searchOrder == "asc" ? DemoOrder.TitleAsc : DemoOrder.TitleDesc,
                    "date" => searchOrder == "asc" ? DemoOrder.CreatedAsc : DemoOrder.CreatedDesc,
                    "uploadedBy" => searchOrder == "asc" ? DemoOrder.UploadedByAsc : DemoOrder.UploadedByDesc,
                    _ => DemoOrder.CreatedDesc
                };
            }

            return order;
        }
    }

    /// <summary>
    /// DTO for demo data transferred via API endpoints
    /// </summary>
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
