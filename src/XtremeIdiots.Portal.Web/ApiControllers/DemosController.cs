using System.Globalization;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Demos;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.ApiControllers;

[Authorize(Policy = AuthPolicies.AccessDemos)]
[Route("Demos")]
public class DemosController : BaseApiController
{
    private readonly IAuthorizationService authorizationService;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly UserManager<IdentityUser> userManager;

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

                filterUserId = gameTypes.Contains((GameType)id) ? null : User.XtremeIdiotsId();
            }
            else
            {
                filterGameTypes = gameTypes.ToArray();

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