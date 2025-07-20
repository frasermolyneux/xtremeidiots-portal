using System.Globalization;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Demos;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.ApiControllers;
/// <summary>
/// RESTful API controller providing demo management endpoints for the XtremeIdiots Portal.
/// Handles demo listing, client authentication, and authorization-based data access for Call of Duty game demos.
/// Supports both web-based DataTable AJAX requests and external demo manager client applications.
/// </summary>
/// <remarks>
/// This controller serves as the API bridge for demo operations, providing:
/// - Authorization-filtered demo lists based on user claims and game types
/// - External client authentication via demo auth keys
/// - DataTable-compatible JSON responses with sorting, filtering, and pagination
/// - Integration with Repository API for demo data retrieval
/// - Comprehensive telemetry tracking for monitoring demo access patterns
/// 
/// Authorization is enforced at both controller and action levels, with game-specific filtering
/// applied based on user claims (SeniorAdmin, HeadAdmin, GameAdmin, Moderator).
/// External clients authenticate using demo auth keys stored in user profiles.
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessDemos)]
[Route("Demos")]
public class DemosController : BaseApiController
{
    private readonly IAuthorizationService authorizationService;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly UserManager<IdentityUser> userManager;

    /// <summary>
    /// Initializes a new instance of the DemosController with required dependencies for demo management operations.
    /// </summary>
    /// <param name="authorizationService">Service for handling authorization checks and policy enforcement</param>
    /// <param name="userManager">Manager for ASP.NET Core Identity user operations and user retrieval</param>
    /// <param name="signInManager">Manager for sign-in operations and claims principal creation</param>
    /// <param name="repositoryApiClient">Client for Repository API operations and demo data retrieval</param>
    /// <param name="telemetryClient">Client for Application Insights telemetry and performance monitoring</param>
    /// <param name="logger">Logger instance for structured logging and error tracking</param>
    /// <param name="configuration">Configuration service for application settings and demo manager integration</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
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
    /// Provides demo list data as JSON for DataTable AJAX requests with authorization-based filtering.
    /// Supports game-specific filtering, user-based access control, and comprehensive search functionality.
    /// </summary>
    /// <param name="id">Optional game type to filter demos by specific Call of Duty game</param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation</param>
    /// <returns>
    /// JSON response with demo data formatted for DataTables consumption including:
    /// - Paginated demo records with sorting and filtering applied
    /// - Authorization-based action links (delete permissions)
    /// - Total and filtered record counts for DataTable pagination
    /// - Enriched demo data with user profile information
    /// </returns>
    /// <exception cref="BadRequestResult">Thrown when the DataTable request model is invalid or malformed</exception>
    /// <exception cref="StatusCodeResult">Thrown when Repository API fails to retrieve demo data</exception>
    /// <remarks>
    /// This endpoint implements game-specific authorization where users can only see demos from games
    /// they have admin/moderator claims for. Users without admin claims can only see their own demos.
    /// The response includes authorization context to show/hide action buttons in the UI.
    /// </remarks>
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
            if (id is not null)
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
            if (demosApiResponse.Result.Data.Items is not null)
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

            TrackSuccessTelemetry("DemoListLoaded", nameof(GetDemoListAjax), new Dictionary<string, string>
            {
                    { "GameType", id?.ToString() ?? "All" },
                    { "ResultCount", portalDemoEntries.Count.ToString(CultureInfo.InvariantCulture) },
                    { "TotalCount", demosApiResponse.Result.Data.TotalCount.ToString(CultureInfo.InvariantCulture) }
            });

            return Ok(new
            {
                model.Draw,
                recordsTotal = demosApiResponse.Result.Data.TotalCount,
                recordsFiltered = demosApiResponse.Result.Data.FilteredCount,
                data = portalDemoEntries
            });
        }, nameof(GetDemoListAjax));
    }

    /// <summary>
    /// Provides API endpoint for external demo manager client applications to retrieve demo list.
    /// Authenticates clients using demo auth keys and returns authorization-filtered demo data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation</param>
    /// <returns>
    /// JSON array of demos for authenticated clients, or error message for authentication failures.
    /// Demo data includes game information, file details, and server context for client processing.
    /// </returns>
    /// <exception cref="ContentResult">Returns authentication error messages when auth key validation fails</exception>
    /// <exception cref="ContentResult">Returns server error message when demo retrieval fails</exception>
    /// <remarks>
    /// This endpoint serves external demo manager applications that require programmatic access to demo data.
    /// Authentication is performed via 'demo-manager-auth-key' header containing user-specific auth keys.
    /// Users can only access demos from games they have admin/moderator claims for, or their own demos.
    /// Returns up to 500 most recent demos ordered by creation date descending.
    /// 
    /// Error responses are returned as plain text for easier client parsing, while successful responses
    /// use JSON format with demo metadata compatible with demo manager client applications.
    /// </remarks>
    [HttpGet("ClientDemoList")]
    public async Task<IActionResult> ClientDemoList(CancellationToken cancellationToken = default)
    {
        try
        {
            const string authKeyHeader = "demo-manager-auth-key";

            if (!Request.Headers.ContainsKey(authKeyHeader))
            {
                Logger.LogDebug("{MethodName} - No auth key provided in request headers", nameof(ClientDemoList));
                return Content("AuthError: No auth key provided in the request. This should be set in the client.");
            }

            var authKey = Request.Headers[authKeyHeader].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                Logger.LogDebug("{MethodName} - Auth key header supplied but was empty", nameof(ClientDemoList));
                return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("{MethodName} - Invalid auth key provided: {AuthKeyPrefix}", nameof(ClientDemoList), authKey.Substring(0, Math.Min(4, authKey.Length)));
                return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");
            }

            var userIdFromProfile = userProfileApiResponse.Result.Data.XtremeIdiotsForumId;

            if (string.IsNullOrWhiteSpace(userIdFromProfile))
            {
                Logger.LogError("{MethodName} - User profile missing XtremeIdiotsForumId for profile {UserProfileId}", nameof(ClientDemoList), userProfileApiResponse.Result.Data.UserProfileId);
                return Content("AuthError: An internal auth error occurred processing your request - missing user ID.");
            }

            var user = await userManager.FindByIdAsync(userIdFromProfile);
            if (user is null)
            {
                Logger.LogWarning("{MethodName} - User not found for ID {UserId}", nameof(ClientDemoList), userIdFromProfile);
                return Content($"AuthError: An internal auth error occurred processing your request for userId: {userIdFromProfile}");
            }

            var claimsPrincipal = await signInManager.ClaimsFactory.CreateAsync(user);

            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.Moderator };
            var gameTypes = claimsPrincipal.ClaimedGameTypes(requiredClaims);

            string? filterUserId = null;
            GameType[]? filterGameTypes = gameTypes.ToArray();
            if (!gameTypes.Any()) filterUserId = userIdFromProfile;

            var demosApiResponse = await repositoryApiClient.Demos.V1.GetDemos(filterGameTypes, filterUserId, null, 0, 500, DemoOrder.CreatedDesc);

            if (!demosApiResponse.IsSuccess || demosApiResponse.Result?.Data?.Items is null)
            {
                Logger.LogError("{MethodName} - Failed to retrieve demos for user {UserId}", nameof(ClientDemoList), userIdFromProfile);
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

            Logger.LogInformation("{MethodName} - Successfully provided {DemoCount} demos to client for user {UserId}", nameof(ClientDemoList), demos.Count, userIdFromProfile);

            var clientListTelemetry = new EventTelemetry("ClientDemoListProvided");
            clientListTelemetry.Properties.TryAdd("LoggedInAdminId", userIdFromProfile);
            clientListTelemetry.Properties.TryAdd("DemoCount", demos.Count.ToString(CultureInfo.InvariantCulture));
            TelemetryClient.TrackEvent(clientListTelemetry);

            return Ok(demos);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in {MethodName} endpoint", nameof(ClientDemoList));

            var exceptionTelemetry = new ExceptionTelemetry(ex)
            {
                SeverityLevel = SeverityLevel.Error
            };
            exceptionTelemetry.Properties.TryAdd("ActionType", nameof(ClientDemoList));
            TelemetryClient.TrackException(exceptionTelemetry);

            return Content("Error: An internal server error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Helper method to determine the correct DemoOrder enumeration value from DataTable AJAX model.
    /// Maps DataTable column names and sort directions to Repository API sort parameters.
    /// </summary>
    /// <param name="model">The DataTable AJAX model containing order information and column definitions</param>
    /// <returns>The appropriate DemoOrder enum value for Repository API consumption</returns>
    /// <remarks>
    /// Supports sorting by game type, demo title, creation date, and uploaded by user.
    /// Defaults to CreatedDesc (most recent first) when no specific order is provided or column is unrecognized.
    /// This ensures consistent sorting behavior across the demo management interface.
    /// </remarks>
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
/// Data Transfer Object for demo information transferred via API endpoints to client applications.
/// Provides enriched demo data with authorization context and user-friendly formatting for web consumption.
/// </summary>
/// <remarks>
/// This DTO serves as the bridge between Repository API demo data and client application requirements.
/// It includes authorization flags (ShowDeleteLink) that control UI element visibility based on user permissions.
/// User information defaults to Admin (ID: 21145) when user profile data is unavailable for legacy compatibility.
/// </remarks>
public class PortalDemoDto
{
    /// <summary>
    /// Initializes a new instance of PortalDemoDto from Repository API demo data.
    /// </summary>
    /// <param name="demo">The demo data from Repository API containing game and file information</param>
    /// <remarks>
    /// Converts Repository API demo format to portal-specific format with user-friendly property names.
    /// Provides fallback values for user information when user profile data is missing.
    /// </remarks>
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