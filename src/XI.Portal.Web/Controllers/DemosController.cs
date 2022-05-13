﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XI.Demos.Models;
using XI.Portal.Demos.Extensions;
using XI.Portal.Demos.Forums;
using XI.Portal.Demos.Interfaces;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Extensions;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessDemos)]
    public class DemosController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDemoAuthRepository _demoAuthRepository;
        private readonly IDemoFileRepository _demosRepository;
        private readonly ILogger<DemosController> _logger;
        private readonly SignInManager<PortalIdentityUser> _signInManager;
        private readonly IDemosForumsClient _demosForumsClient;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly UserManager<PortalIdentityUser> _userManager;

        public DemosController(
            ILogger<DemosController> logger,
            IAuthorizationService authorizationService,
            IDemoFileRepository demosRepository,
            IDemoAuthRepository demoAuthRepository,
            UserManager<PortalIdentityUser> userManager,
            SignInManager<PortalIdentityUser> signInManager,
            IDemosForumsClient demosForumsClient,
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger;
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _demosRepository = demosRepository ?? throw new ArgumentNullException(nameof(demosRepository));
            _demoAuthRepository = demoAuthRepository ?? throw new ArgumentNullException(nameof(demoAuthRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _demosForumsClient = demosForumsClient ?? throw new ArgumentNullException(nameof(demosForumsClient));
            this.repositoryTokenProvider = repositoryTokenProvider;
            this.repositoryApiClient = repositoryApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> DemoClient()
        {
            ViewData["ClientAuthKey"] = await _demoAuthRepository.GetAuthKey(User.XtremeIdiotsId());

            var demoManagerClientDto = await _demosForumsClient.GetDemoManagerClient();
            return View(demoManagerClientDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateAuthKey()
        {
            await _demoAuthRepository.UpdateAuthKey(User.XtremeIdiotsId(), Guid.NewGuid().ToString());

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
        public async Task<IActionResult> GetDemoListAjax(string? id)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator };
            var gameTypes = User.ClaimedGameTypes(requiredClaims);

            string filterUserId = null;
            string[] filterGameTypes = null;
            if (id != null)
            {
                // If the user has the required claims do not filter by user id
                filterUserId = gameTypes.Contains(id) ? null : User.XtremeIdiotsId();
            }
            else
            {
                filterGameTypes = gameTypes.ToArray();

                // If the user has any required claims for games do not filter by user id
                if (!gameTypes.Any()) filterUserId = User.XtremeIdiotsId();
            }

            string order = "DateDesc";
            if (model.Order != null)
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "game":
                        order = searchOrder == "asc" ? "GameTypeAsc" : "GameTypeDesc";
                        break;
                    case "name":
                        order = searchOrder == "asc" ? "NameAsc" : "NameDesc";
                        break;
                    case "date":
                        order = searchOrder == "asc" ? "DateAsc" : "DateDesc";
                        break;
                    case "uploadedBy":
                        order = searchOrder == "asc" ? "UploadedByAsc" : "UploadedByDesc";
                        break;
                }
            }

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            DemosSearchResponseDto searchResponse = await repositoryApiClient.Demos.SearchDemos(accessToken, filterGameTypes, filterUserId, model.Search?.Value, model.Start, model.Length, order);

            var portalDemoEntries = new List<PortalDemoDto>();
            foreach (var demoDto in searchResponse.Entries)
            {
                var canDeletePortalDemo = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, string>(demoDto.Game, demoDto.UserId), AuthPolicies.DeleteDemo);

                var portalDemoDto = new PortalDemoDto(demoDto);

                if (canDeletePortalDemo.Succeeded)
                    portalDemoDto.ShowDeleteLink = true;

                portalDemoEntries.Add(portalDemoDto);
            }

            return Json(new
            {
                model.Draw,
                recordsTotal = searchResponse.TotalRecords,
                recordsFiltered = searchResponse.FilteredRecords,
                data = portalDemoEntries
            });
        }

        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            DemoDto demoDto = await repositoryApiClient.Demos.GetDemo(accessToken, id);

            var demoUrl = await _demosRepository.GetDemoUrl(demoDto.FileName);

            return Redirect(demoUrl.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id, bool filterGame = false)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            DemoDto demoDto = await repositoryApiClient.Demos.GetDemo(accessToken, id);

            if (demoDto == null) return NotFound();

            var canDeleteDemo = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, string>(demoDto.Game, demoDto.UserId), AuthPolicies.DeleteDemo);

            if (!canDeleteDemo.Succeeded)
                return Unauthorized();

            ViewData["FilterGame"] = filterGame;

            return View(demoDto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, bool filterGame = false)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            DemoDto demoDto = await repositoryApiClient.Demos.GetDemo(accessToken, id);

            if (demoDto == null) return NotFound();

            var canDeleteDemo = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, string>(demoDto.Game, demoDto.UserId), AuthPolicies.DeleteDemo);

            if (!canDeleteDemo.Succeeded)
                return Unauthorized();

            await repositoryApiClient.Demos.DeleteDemo(accessToken, id);

            _logger.LogInformation("User {User} has deleted {DemoId} under {GameType}", User.Username(), demoDto.DemoId, demoDto.Game);
            this.AddAlertSuccess($"The demo {demoDto.Name} has been successfully deleted from {demoDto.Game}");

            if (filterGame)
                return RedirectToAction(nameof(GameIndex), new { id = demoDto.Game });
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

            var userId = await _demoAuthRepository.GetUserId(authKey);
            if (userId == null) return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Content($"AuthError: An internal auth error occured processing your request for userId: {userId}");

            var claimsPrincipal = await _signInManager.ClaimsFactory.CreateAsync(user);

            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator };
            var gameTypes = claimsPrincipal.ClaimedGameTypes(requiredClaims);

            string filterUserId = null;
            string[] filterGameTypes = null;

            filterGameTypes = gameTypes.ToArray();
            if (!gameTypes.Any()) filterUserId = User.XtremeIdiotsId();

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            DemosSearchResponseDto searchResponse = await repositoryApiClient.Demos.SearchDemos(accessToken, filterGameTypes, filterUserId, null, 0, 0, "DateDesc");

            var demos = searchResponse.Entries.Select(demo => new
            {
                demo.DemoId,
                Version = demo.Game.ToString(),
                demo.Name,
                demo.Date,
                demo.Map,
                demo.Mod,
                demo.GameType,
                demo.Server,
                demo.Size,
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

            var userId = await _demoAuthRepository.GetUserId(authKey);
            if (userId == null) return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");

            var user = await _userManager.FindByIdAsync(userId);

            if (file == null || file.Length == 0) return Content("You must provide a file to be uploaded");

            var whitelistedExtensions = new List<string> { ".dm_1", ".dm_6" };

            if (!whitelistedExtensions.Any(ext => file.FileName.EndsWith(ext)))
                return Content("Invalid file type - this must be a demo file");

            var gameTypeHeader = Request.Headers["demo-manager-game-type"].ToString();
            Enum.TryParse(gameTypeHeader, out GameType gameType);

            var fileName = $"{Guid.NewGuid()}.{gameType.DemoExtension()}";
            var path = Path.Combine(Path.GetTempPath(), fileName);

            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var localDemo = new LocalDemo(path, gameType);
            var frontEndFileName = Path.GetFileNameWithoutExtension(file.FileName);

            var demoDto = new DemoDto
            {
                Game = gameType,
                Name = frontEndFileName,
                FileName = fileName,
                Date = localDemo.Date,
                Map = localDemo.Map,
                Mod = localDemo.Mod,
                GameType = localDemo.GameType,
                Server = localDemo.Server,
                Size = localDemo.Size,
                UserId = user.Id
            };

            await _demosRepository.CreateDemo(demoDto.FileName, path);

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            await repositoryApiClient.Demos.CreateDemo(accessToken, demoDto);

            _logger.LogInformation("User {Username} has uploaded a new demo {FileName}", user.UserName, demoDto.FileName);

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

            var userId = await _demoAuthRepository.GetUserId(authKey);
            if (userId == null) return Content("AuthError: Your auth key is incorrect, check the portal for the correct one and re-enter it on your client.");

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            DemoDto demoDto = await repositoryApiClient.Demos.GetDemo(accessToken, id);

            var demoUrl = await _demosRepository.GetDemoUrl(demoDto.FileName);

            return Redirect(demoUrl.ToString());
        }

        public class PortalDemoDto
        {
            public PortalDemoDto(DemoDto demo)
            {
                DemoId = demo.DemoId;
                Game = demo.Game.ToString();
                Name = demo.Name;
                FileName = demo.FileName;
                Date = demo.Date;
                Map = demo.Map;
                Mod = demo.Mod;
                GameType = demo.GameType;
                Server = demo.Server;
                Size = demo.Size;
                UserId = demo.UserId;
                UploadedBy = demo.UploadedBy;
            }

            public Guid DemoId { get; set; }
            public string Game { get; set; }
            public string Name { get; set; }
            public string FileName { get; set; }
            public DateTime Date { get; set; }
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