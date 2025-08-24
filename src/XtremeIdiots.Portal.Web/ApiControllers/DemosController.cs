using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Demos;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.ApiControllers;

[Authorize(Policy = AuthPolicies.AccessDemos)]
[Route("Demos")]
public class DemosController(
        IAuthorizationService authorizationService,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IRepositoryApiClient repositoryApiClient,
        TelemetryClient telemetryClient,
        ILogger<DemosController> logger,
        IConfiguration configuration) : BaseApiController(telemetryClient, logger, configuration)
{
    private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    private readonly SignInManager<IdentityUser> signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    private readonly UserManager<IdentityUser> userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

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
                filterGameTypes = [(GameType)id];

                filterUserId = gameTypes.Contains((GameType)id) ? null : User.XtremeIdiotsId();
            }
            else
            {
                filterGameTypes = [.. gameTypes];

                if (gameTypes.Count == 0)
                    filterUserId = User.XtremeIdiotsId();
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
                    { "TotalCount", demosApiResponse.Result?.Pagination?.TotalCount.ToString(CultureInfo.InvariantCulture) ?? "0"}
            });

            return Ok(new
            {
                model.Draw,
                recordsTotal = demosApiResponse.Result?.Pagination?.TotalCount,
                recordsFiltered = demosApiResponse.Result?.Pagination?.FilteredCount,
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

            if (!Request.Headers.TryGetValue(authKeyHeader, out var value))
            {
                Logger.LogDebug("{MethodName} - No auth key provided in request headers", nameof(ClientDemoList));
                return Content("AuthError: No auth key provided in the request. This should be set in the client.");
            }

            var authKey = value.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                Logger.LogDebug("{MethodName} - Auth key header supplied but was empty", nameof(ClientDemoList));
                return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey, cancellationToken);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("{MethodName} - Invalid auth key provided: {AuthKeyPrefix}", nameof(ClientDemoList), authKey[..Math.Min(4, authKey.Length)]);
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
            GameType[]? filterGameTypes = [.. gameTypes];
            if (gameTypes.Count == 0)
                filterUserId = userIdFromProfile;

            var demosApiResponse = await repositoryApiClient.Demos.V1.GetDemos(filterGameTypes, filterUserId, null, 0, 500, DemoOrder.CreatedDesc, cancellationToken);

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

        if (model.Order != null && model.Order.Count != 0)
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

public class PortalDemoDto(DemoDto demo)
{
    public Guid DemoId { get; set; } = demo.DemoId;
    public string Game { get; set; } = demo.GameType.ToString();
    public string Name { get; set; } = demo.Title;
    public string FileName { get; set; } = demo.FileName;
    public DateTime? Date { get; set; } = demo.Created;
    public string Map { get; set; } = demo.Map;
    public string Mod { get; set; } = demo.Mod;
    public string GameType { get; set; } = demo.GameMode;
    public string Server { get; set; } = demo.ServerName;
    public long Size { get; set; } = demo.FileSize;
    public string UserId { get; set; } = demo.UserProfile?.XtremeIdiotsForumId ?? "21145";
    public string UploadedBy { get; set; } = demo.UserProfile?.DisplayName ?? "Admin";

    public bool ShowDeleteLink { get; set; }
}