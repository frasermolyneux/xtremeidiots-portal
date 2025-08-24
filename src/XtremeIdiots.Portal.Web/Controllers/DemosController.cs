using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Demos;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Manages demo file operations including viewing, deletion, and authentication key management
/// </summary>
/// <remarks>
/// This controller handles game demo files with role-based access control. Users can manage their own demos
/// or access all demos based on their authorization level. Integrates with forums for demo client management.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the DemosController
/// </remarks>
/// <param name="authorizationService">Service for handling authorization checks</param>
/// <param name="userManager">ASP.NET Identity user manager</param>
/// <param name="signInManager">ASP.NET Identity sign-in manager</param>
/// <param name="demosForumsClient">Client for forums integration demo management</param>
/// <param name="repositoryApiClient">Client for repository API operations</param>
/// <param name="telemetryClient">Application insights telemetry client</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
[Authorize(Policy = AuthPolicies.AccessDemos)]
public class DemosController(
    IAuthorizationService authorizationService,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IDemoManager demosForumsClient,
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<DemosController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    private readonly UserManager<IdentityUser> userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    private readonly SignInManager<IdentityUser> signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    private readonly IDemoManager demosForumsClient = demosForumsClient ?? throw new ArgumentNullException(nameof(demosForumsClient));
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Displays the demo client configuration page with authentication key information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with demo client configuration and user authentication key</returns>
    [HttpGet]
    public async Task<IActionResult> DemoClient(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var userId = User.XtremeIdiotsId();
            Logger.LogInformation("User {UserId} accessing demo client configuration page", userId);

            var hasAuthKey = false;

            if (!string.IsNullOrEmpty(userId))
            {
                var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(userId);

                if (!userProfileApiResponse.IsNotFound && userProfileApiResponse.Result?.Data is not null)
                {
                    ViewData["ClientAuthKey"] = userProfileApiResponse.Result.Data.DemoAuthKey;
                    hasAuthKey = true;
                }
            }

            var demoManagerClientDto = await demosForumsClient.GetDemoManagerClient();

            TrackSuccessTelemetry(nameof(DemoClient), nameof(DemoClient), new Dictionary<string, string>
            {
                { nameof(DemosController), nameof(DemosController) },
                { "Resource", nameof(DemoClient) },
                { "Context", "DemoClientAccess" },
                { "HasAuthKey", hasAuthKey.ToString() }
            });

            return View(demoManagerClientDto);
        }, $"Display {nameof(DemoClient)} configuration page with authentication key");
    }

    /// <summary>
    /// Regenerates the demo authentication key for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to demo client page with success message on completion</returns>
    /// <exception cref="ArgumentException">Thrown when user profile is not found</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegenerateAuthKey(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var userId = User.XtremeIdiotsId();
            Logger.LogInformation("User {UserId} attempting to regenerate demo auth key", userId);

            if (string.IsNullOrEmpty(userId))
            {
                Logger.LogWarning("User ID is null when regenerating demo auth key");
                return NotFound();
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(userId);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("User profile {UserId} not found when regenerating demo auth key", userId);
                return NotFound();
            }

            var editUserProfileDto = new EditUserProfileDto(userProfileApiResponse.Result.Data.UserProfileId)
            {
                DemoAuthKey = Guid.NewGuid().ToString()
            };

            await repositoryApiClient.UserProfiles.V1.UpdateUserProfile(editUserProfileDto);

            this.AddAlertSuccess("Your demo auth key has been regenerated, you will need to reconfigure your client desktop application");

            TrackSuccessTelemetry(nameof(RegenerateAuthKey), nameof(RegenerateAuthKey), new Dictionary<string, string>
            {
                { nameof(DemosController), nameof(DemosController) },
                { "Resource", "DemoAuthKey" },
                { "Context", nameof(RegenerateAuthKey) },
                { "UserProfileId", userProfileApiResponse.Result.Data.UserProfileId.ToString() }
            });

            return RedirectToAction(nameof(DemoClient));
        }, $"Regenerate demo authentication key for user");
    }

    /// <summary>
    /// Displays the main demos index page
    /// </summary>
    /// <returns>View for browsing all available demo files</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            await Task.CompletedTask;

            TrackSuccessTelemetry(nameof(Index), nameof(Index), new Dictionary<string, string>
            {
                { nameof(DemosController), nameof(DemosController) },
                { "Resource", nameof(Index) }
            });

            return View();
        }, nameof(Index));
    }

    /// <summary>
    /// Displays demos filtered by specific game type
    /// </summary>
    /// <param name="id">The game type to filter demos by</param>
    /// <returns>View with demos filtered by the specified game type</returns>
    [HttpGet]
    public async Task<IActionResult> GameIndex(GameType? id)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            await Task.CompletedTask;

            ViewData["GameType"] = id;

            TrackSuccessTelemetry(nameof(GameIndex), nameof(GameIndex), new Dictionary<string, string>
            {
                { nameof(DemosController), nameof(DemosController) },
                { "Resource", nameof(GameIndex) },
                { "GameType", id?.ToString() ?? "null" }
            });

            return View(nameof(Index));
        }, nameof(GameIndex));
    }

    /// <summary>
    /// Handles AJAX requests for demo list data with pagination and filtering
    /// </summary>
    /// <param name="id">Optional game type filter for demos</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>JSON response with paginated demo data for DataTables</returns>
    [HttpPost]
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
                return RedirectToAction("Display", "Errors", new { id = 500 });
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

            TrackSuccessTelemetry(nameof(GetDemoListAjax), nameof(GetDemoListAjax), new Dictionary<string, string>
            {
                { "GameType", id?.ToString() ?? "All" },
                { "ResultCount", portalDemoEntries.Count.ToString() },
                { "TotalCount", demosApiResponse.Result.Pagination.TotalCount.ToString() }
            });

            return Json(new
            {
                model.Draw,
                recordsTotal = demosApiResponse.Result.Pagination.TotalCount,
                recordsFiltered = demosApiResponse.Result.Pagination.FilteredCount,
                data = portalDemoEntries
            });
        }, nameof(GetDemoListAjax));
    }

    private static DemoOrder GetDemoOrderFromDataTable(DataTableAjaxPostModel model)
    {
        var order = DemoOrder.CreatedDesc;

        if (model.Order is not null && model.Order.Count != 0)
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

    [HttpGet]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var demoApiResult = await repositoryApiClient.Demos.V1.GetDemo(id, cancellationToken);

            if (demoApiResult.IsNotFound || demoApiResult.Result?.Data is null)
            {
                Logger.LogWarning("Demo {DemoId} not found when user {UserId} attempted download", id, User.XtremeIdiotsId());

                TrackUnauthorizedAccessAttempt(nameof(Download), "Demo", $"DemoId:{id},Reason:NotFound", new { DemoId = id });

                return NotFound();
            }

            TrackSuccessTelemetry(nameof(Download), nameof(Download), new Dictionary<string, string>
        {
 { "DemoId", id.ToString() },
 { "DemoTitle", demoApiResult.Result.Data.Title },
 { "GameType", demoApiResult.Result.Data.GameType.ToString() }
        });

            return Redirect(demoApiResult.Result.Data.FileUri);
        }, nameof(Download));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, bool filterGame = false, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var demoApiResult = await repositoryApiClient.Demos.V1.GetDemo(id, cancellationToken);

            if (demoApiResult.IsNotFound || demoApiResult.Result?.Data is null)
            {
                return NotFound();
            }

            var authorizationResource = new Tuple<GameType, Guid>(demoApiResult.Result.Data.GameType, demoApiResult.Result.Data.UserProfileId);
            var authorizationResult = await CheckAuthorizationAsync(authorizationService, authorizationResource, AuthPolicies.DeleteDemo, nameof(Delete), "Demo", $"GameType:{demoApiResult.Result.Data.GameType}");
            if (authorizationResult != null)
                return authorizationResult;

            ViewData["FilterGame"] = filterGame;

            TrackSuccessTelemetry(nameof(Delete), nameof(Delete), new Dictionary<string, string>
        {
 { "DemoId", id.ToString() },
 { "GameType", demoApiResult.Result.Data.GameType.ToString() },
 { "FilterGame", filterGame.ToString() }
        });

            return View(demoApiResult.Result);
        }, nameof(Delete));
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, bool filterGame = false, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var demoApiResult = await repositoryApiClient.Demos.V1.GetDemo(id, cancellationToken);

            if (demoApiResult.IsNotFound || demoApiResult.Result?.Data is null)
            {
                return NotFound();
            }

            var authorizationResource = new Tuple<GameType, Guid>(demoApiResult.Result.Data.GameType, demoApiResult.Result.Data.UserProfileId);
            var authorizationResult = await CheckAuthorizationAsync(authorizationService, authorizationResource, AuthPolicies.DeleteDemo, nameof(DeleteConfirmed), "Demo", $"GameType:{demoApiResult.Result.Data.GameType}");
            if (authorizationResult != null)
                return authorizationResult;

            await repositoryApiClient.Demos.V1.DeleteDemo(id, cancellationToken);

            this.AddAlertSuccess($"The demo {demoApiResult.Result.Data.Title} has been successfully deleted from {demoApiResult.Result.Data.GameType}");

            TrackSuccessTelemetry(nameof(DeleteConfirmed), nameof(DeleteConfirmed), new Dictionary<string, string>
        {
 { "DemoId", id.ToString() },
 { "DemoTitle", demoApiResult.Result.Data.Title },
 { "GameType", demoApiResult.Result.Data.GameType.ToString() }
        });

            return filterGame
                ? RedirectToAction(nameof(GameIndex), new { id = demoApiResult.Result.Data.GameType })
                : RedirectToAction(nameof(Index));
        }, nameof(DeleteConfirmed));
    }

    [HttpGet]
    public async Task<IActionResult> ClientDemoList(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Request.Headers.TryGetValue("demo-manager-auth-key", out var value))
            {
                Logger.LogDebug("ClientDemoList - No auth key provided in request headers");
                return Content("AuthError: No auth key provided in the request. This should be set in the client.");
            }

            var authKey = value.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                Logger.LogDebug("ClientDemoList - Auth key header supplied but was empty");
                return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey, cancellationToken);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("ClientDemoList - Invalid auth key provided: {AuthKeyPrefix}", authKey[..Math.Min(4, authKey.Length)]);
                return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");
            }

            var userIdFromProfile = userProfileApiResponse.Result.Data.XtremeIdiotsForumId;
            if (string.IsNullOrWhiteSpace(userIdFromProfile))
            {
                Logger.LogError("ClientDemoList - User profile missing XtremeIdiotsForumId for profile {UserProfileId}", userProfileApiResponse.Result.Data.UserProfileId);
                return Content("AuthError: An internal auth error occurred processing your request - missing user ID.");
            }

            var user = await userManager.FindByIdAsync(userIdFromProfile);
            if (user is null)
            {
                Logger.LogWarning("ClientDemoList - User not found for ID {UserId}", userIdFromProfile);
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

            return Json(demos);
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

    [HttpPost]
    public async Task<ActionResult> ClientUploadDemo(IFormFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Request.Headers.TryGetValue("demo-manager-auth-key", out var value))
            {
                Logger.LogDebug("ClientUploadDemo - No auth key provided in request headers");
                return Content("AuthError: No auth key provided in the request. This should be set in the client.");
            }

            var authKey = value.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                Logger.LogDebug("ClientUploadDemo - Auth key header supplied was empty");
                return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey, cancellationToken);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("ClientUploadDemo - Invalid auth key provided: {AuthKeyPrefix}", authKey[..Math.Min(4, authKey.Length)]);
                return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");
            }

            var userIdFromProfile = userProfileApiResponse.Result.Data.XtremeIdiotsForumId;
            if (string.IsNullOrWhiteSpace(userIdFromProfile))
            {
                Logger.LogError("ClientUploadDemo - User profile missing XtremeIdiotsForumId for profile {UserProfileId}", userProfileApiResponse.Result.Data.UserProfileId);
                return Content("AuthError: An internal auth error occurred processing your request - missing user ID.");
            }

            var gameTypeHeader = Request.Headers["demo-manager-game-type"].ToString();
            if (!Enum.TryParse(gameTypeHeader, out GameType gameType))
            {
                Logger.LogWarning("ClientUploadDemo - Invalid or missing game type header: {GameTypeHeader}", gameTypeHeader);
                return Content("Error: Invalid or missing game type. Please ensure your client is properly configured.");
            }

            if (file is null || file.Length == 0)
            {
                Logger.LogWarning("ClientUploadDemo - No file provided by user {UserId}", userIdFromProfile);
                return Content("You must provide a file to be uploaded");
            }

            var whitelistedExtensions = new List<string> { ".dm_1", ".dm_6" };

            if (!whitelistedExtensions.Any(file.FileName.EndsWith))
            {
                Logger.LogWarning("ClientUploadDemo - Invalid file extension {FileName} by user {UserId}", file.FileName, userIdFromProfile);
                return Content("Invalid file type extension");
            }

            var filePath = Path.Join(Path.GetTempPath(), file.FileName);
            using (var stream = System.IO.File.Create(filePath))
                await file.CopyToAsync(stream, cancellationToken);

            var demoDto = new CreateDemoDto(gameType, userProfileApiResponse.Result.Data.UserProfileId);

            var createDemoApiResponse = await repositoryApiClient.Demos.V1.CreateDemo(demoDto, cancellationToken);
            if (createDemoApiResponse.IsSuccess && createDemoApiResponse.Result?.Data != null)
            {
                await repositoryApiClient.Demos.V1.SetDemoFile(createDemoApiResponse.Result.Data.DemoId, file.FileName, filePath, cancellationToken);
            }

            Logger.LogInformation("User {UserId} successfully uploaded demo {FileName} for game type {GameType}",
            userIdFromProfile, file.FileName, gameType);

            var clientUploadTelemetry = new EventTelemetry("ClientDemoUploaded");
            clientUploadTelemetry.Properties.TryAdd("LoggedInAdminId", userIdFromProfile);
            clientUploadTelemetry.Properties.TryAdd("FileName", file.FileName);
            clientUploadTelemetry.Properties.TryAdd("GameType", gameType.ToString());
            clientUploadTelemetry.Properties.TryAdd("FileSize", file.Length.ToString());
            TelemetryClient.TrackEvent(clientUploadTelemetry);

            return Ok();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in ClientUploadDemo endpoint");

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

            TelemetryClient.TrackException(exceptionTelemetry);

            return Content("Error: An internal server error occurred while uploading your demo.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ClientDownload(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Request.Headers.TryGetValue("demo-manager-auth-key", out var value))
            {
                Logger.LogDebug("ClientDownload - No auth key provided in request headers");
                return Content("AuthError: No auth key provided in the request. This should be set in the client.");
            }

            var authKey = value.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                Logger.LogDebug("ClientDownload - Auth key header supplied but was empty");
                return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey, cancellationToken);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("ClientDownload - Invalid auth key provided: {AuthKeyPrefix}", authKey[..Math.Min(4, authKey.Length)]);
                return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");
            }

            var userIdFromProfile = userProfileApiResponse.Result.Data.XtremeIdiotsForumId;
            if (string.IsNullOrWhiteSpace(userIdFromProfile))
            {
                Logger.LogError("ClientDownload - User profile missing XtremeIdiotsForumId for profile {UserProfileId}", userProfileApiResponse.Result.Data.UserProfileId);
                return Content("AuthError: An internal auth error occurred processing your request - missing user ID.");
            }

            var demoApiResponse = await repositoryApiClient.Demos.V1.GetDemo(id, cancellationToken);

            if (demoApiResponse.IsNotFound || demoApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("ClientDownload - Demo {DemoId} not found for user {UserId}", id, userIdFromProfile);
                return NotFound();
            }

            Logger.LogInformation("User {UserId} successfully downloading demo {DemoId} via client", userIdFromProfile, id);

            var clientDownloadTelemetry = new EventTelemetry("ClientDemoDownloaded")
            .Enrich(demoApiResponse.Result.Data);
            clientDownloadTelemetry.Properties.TryAdd("LoggedInAdminId", userIdFromProfile);
            TelemetryClient.TrackEvent(clientDownloadTelemetry);

            return Redirect(demoApiResponse.Result.Data.FileUri);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in ClientDownload endpoint for demo {DemoId}", id);

            var exceptionTelemetry = new ExceptionTelemetry(ex)
            {
                SeverityLevel = SeverityLevel.Error
            };
            exceptionTelemetry.Properties.TryAdd("DemoId", id.ToString());
            exceptionTelemetry.Properties.TryAdd("ActionType", "ClientDownload");
            TelemetryClient.TrackException(exceptionTelemetry);

            return Content("Error: An internal server error occurred while downloading the demo.");
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
}