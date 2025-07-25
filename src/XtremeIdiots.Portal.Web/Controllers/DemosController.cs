﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessDemos)]
    public class DemosController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<DemosController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IDemoManager _demosForumsClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly UserManager<IdentityUser> _userManager;

        public DemosController(
            ILogger<DemosController> logger,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IDemoManager demosForumsClient,
            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger;
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _demosForumsClient = demosForumsClient ?? throw new ArgumentNullException(nameof(demosForumsClient));
            this.repositoryApiClient = repositoryApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> DemoClient()
        {
            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(User.XtremeIdiotsId());

            if (!userProfileApiResponse.IsNotFound && userProfileApiResponse.Result != null)
                ViewData["ClientAuthKey"] = userProfileApiResponse.Result.DemoAuthKey;

            var demoManagerClientDto = await _demosForumsClient.GetDemoManagerClient();
            return View(demoManagerClientDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateAuthKey()
        {
            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(User.XtremeIdiotsId());

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result == null)
                return NotFound();

            var editUserProfileDto = new EditUserProfileDto(userProfileApiResponse.Result.UserProfileId)
            {
                DemoAuthKey = Guid.NewGuid().ToString()
            };

            await repositoryApiClient.UserProfiles.V1.UpdateUserProfile(editUserProfileDto);

            _logger.LogInformation("User {User} has regenerated their demo auth key", User.Username());
            this.AddAlertSuccess("Your demo auth key has been regenerated, you will need to reconfigure your client desktop application");

            return RedirectToAction(nameof(DemoClient));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GameIndex(GameType? id)
        {
            ViewData["GameType"] = id;
            return View(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetDemoListAjax(GameType? id)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

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

            var demosApiResponse = await repositoryApiClient.Demos.V1.GetDemos(filterGameTypes, filterUserId, model.Search?.Value, model.Start, model.Length, order);

            if (!demosApiResponse.IsSuccess || demosApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var portalDemoEntries = new List<PortalDemoDto>();
            foreach (var demoDto in demosApiResponse.Result.Entries)
            {
                var canDeletePortalDemo = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(demoDto.GameType, demoDto.UserProfileId), AuthPolicies.DeleteDemo);

                var portalDemoDto = new PortalDemoDto(demoDto);

                if (canDeletePortalDemo.Succeeded)
                    portalDemoDto.ShowDeleteLink = true;

                portalDemoEntries.Add(portalDemoDto);
            }

            return Json(new
            {
                model.Draw,
                recordsTotal = demosApiResponse.Result.TotalRecords,
                recordsFiltered = demosApiResponse.Result.FilteredRecords,
                data = portalDemoEntries
            });
        }

        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            var demoApiResult = await repositoryApiClient.Demos.V1.GetDemo(id);

            if (demoApiResult.IsNotFound || demoApiResult.Result == null)
                return NotFound();

            return Redirect(demoApiResult.Result.FileUri);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id, bool filterGame = false)
        {
            var demoApiResult = await repositoryApiClient.Demos.V1.GetDemo(id);

            if (demoApiResult.IsNotFound || demoApiResult.Result == null)
                return NotFound();

            var canDeleteDemo = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(demoApiResult.Result.GameType, demoApiResult.Result.UserProfileId), AuthPolicies.DeleteDemo);

            if (!canDeleteDemo.Succeeded)
                return Unauthorized();

            ViewData["FilterGame"] = filterGame;

            return View(demoApiResult.Result);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, bool filterGame = false)
        {
            var demoApiResult = await repositoryApiClient.Demos.V1.GetDemo(id);

            if (demoApiResult.IsNotFound || demoApiResult.Result == null)
                return NotFound();

            var canDeleteDemo = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(demoApiResult.Result.GameType, demoApiResult.Result.UserProfileId), AuthPolicies.DeleteDemo);

            if (!canDeleteDemo.Succeeded)
                return Unauthorized();

            await repositoryApiClient.Demos.V1.DeleteDemo(id);

            _logger.LogInformation("User {User} has deleted {DemoId} under {GameType}", User.Username(), demoApiResult.Result.DemoId, demoApiResult.Result.GameType);
            this.AddAlertSuccess($"The demo {demoApiResult.Result.Title} has been successfully deleted from {demoApiResult.Result.GameType}");

            if (filterGame)
                return RedirectToAction(nameof(GameIndex), new { id = demoApiResult.Result.GameType });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ClientDemoList()
        {
            if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                return Content("AuthError: No auth key provided in the request. This should be set in the client.");

            var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                _logger.LogDebug("ClientDemoList - Auth key header supplied but was empty");
                return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result == null)
                return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");

            var userId = userProfileApiResponse.Result.XtremeIdiotsForumId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Content($"AuthError: An internal auth error occured processing your request for userId: {userId}");

            var claimsPrincipal = await _signInManager.ClaimsFactory.CreateAsync(user);

            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.Moderator };
            var gameTypes = claimsPrincipal.ClaimedGameTypes(requiredClaims);

            string? filterUserId = null;
            GameType[]? filterGameTypes = null;

            filterGameTypes = gameTypes.ToArray();
            if (!gameTypes.Any()) filterUserId = User.XtremeIdiotsId();

            var demosApiResponse = await repositoryApiClient.Demos.V1.GetDemos(filterGameTypes, filterUserId, null, 0, 500, DemoOrder.CreatedDesc);

            if (!demosApiResponse.IsSuccess || demosApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var demos = demosApiResponse.Result.Entries.Select(demo => new
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

            return Json(demos);
        }

        [HttpPost]
        public async Task<ActionResult> ClientUploadDemo(IFormFile file)
        {
            if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                return Content("AuthError: No auth key provided in the request. This should be set in the client.");

            var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                _logger.LogDebug("ClientUploadDemo - Auth key header supplied was empty");
                return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result == null)
                return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");

            var userId = userProfileApiResponse.Result.XtremeIdiotsForumId;

            var gameTypeHeader = Request.Headers["demo-manager-game-type"].ToString();
            Enum.TryParse(gameTypeHeader, out GameType gameType);

            if (file == null || file.Length == 0)
                return Content("You must provide a file to be uploaded");

            var whitelistedExtensions = new List<string> { ".dm_1", ".dm_6" };

            if (!whitelistedExtensions.Any(ext => file.FileName.EndsWith(ext)))
                return Content("Invalid file type extension");

            var filePath = Path.Join(Path.GetTempPath(), file.FileName);
            using (var stream = System.IO.File.Create(filePath))
                await file.CopyToAsync(stream);

            var demoDto = new CreateDemoDto(gameType, userProfileApiResponse.Result.UserProfileId);

            var createDemoApiResponse = await repositoryApiClient.Demos.V1.CreateDemo(demoDto);
            if (createDemoApiResponse.IsSuccess && createDemoApiResponse.Result != null)
            {
                await repositoryApiClient.Demos.V1.SetDemoFile(createDemoApiResponse.Result.DemoId, file.FileName, filePath);
            }

            _logger.LogInformation("User {userId} has uploaded a new demo {FileName}", userId, file.FileName);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ClientDownload(Guid id)
        {
            if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                return Content("AuthError: No auth key provided in the request. This should be set in the client.");

            var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                _logger.LogDebug("ClientDownload - Auth key header supplied but was empty");
                return Content("AuthError: The auth key supplied was empty. This should be set in the client.");
            }

            var userProfileApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByDemoAuthKey(authKey);

            if (userProfileApiResponse.IsNotFound || userProfileApiResponse.Result == null)
                return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");

            var demoApiResponse = await repositoryApiClient.Demos.V1.GetDemo(id);

            if (demoApiResponse.IsNotFound || demoApiResponse.Result == null)
                return NotFound();
            else
                return Redirect(demoApiResponse.Result.FileUri);
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