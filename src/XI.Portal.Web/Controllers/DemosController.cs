using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Demos.Models;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Auth.Contract.Models;
using XI.Portal.Auth.Demos.Extensions;
using XI.Portal.Demos.Dto;
using XI.Portal.Demos.Extensions;
using XI.Portal.Demos.Forums;
using XI.Portal.Demos.Interfaces;
using XI.Portal.Demos.Models;
using XI.Portal.Web.Extensions;
using XI.Portal.Web.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessDemos)]
    public class DemosController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDemoAuthRepository _demoAuthRepository;
        private readonly IDemosRepository _demosRepository;
        private readonly ILogger<DemosController> _logger;
        private readonly SignInManager<PortalIdentityUser> _signInManager;
        private readonly IDemosForumsClient _demosForumsClient;
        private readonly UserManager<PortalIdentityUser> _userManager;

        public DemosController(
            ILogger<DemosController> logger,
            IAuthorizationService authorizationService,
            IDemosRepository demosRepository,
            IDemoAuthRepository demoAuthRepository,
            UserManager<PortalIdentityUser> userManager,
            SignInManager<PortalIdentityUser> signInManager,
            IDemosForumsClient demosForumsClient)
        {
            _logger = logger;
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _demosRepository = demosRepository ?? throw new ArgumentNullException(nameof(demosRepository));
            _demoAuthRepository = demoAuthRepository ?? throw new ArgumentNullException(nameof(demoAuthRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _demosForumsClient = demosForumsClient ?? throw new ArgumentNullException(nameof(demosForumsClient));
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

            _logger.LogInformation(EventIds.DemoManager, "User {User} has regenerated their demo auth key", User.Username());
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

            var filterModel = new DemosFilterModel().ApplyAuth(User, id);

            if (id != null)
                filterModel.GameTypes.Add((GameType) id);

            var recordsTotal = await _demosRepository.GetDemosCount(filterModel);

            filterModel.FilterString = model.Search?.Value;
            var recordsFiltered = await _demosRepository.GetDemosCount(filterModel);

            filterModel.TakeEntries = model.Length;
            filterModel.SkipEntries = model.Start;

            if (model.Order == null)
            {
                filterModel.Order = DemosFilterModel.OrderBy.DateDesc;
            }
            else
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "game":
                        filterModel.Order = searchOrder == "asc" ? DemosFilterModel.OrderBy.GameTypeAsc : DemosFilterModel.OrderBy.GameTypeDesc;
                        break;
                    case "name":
                        filterModel.Order = searchOrder == "asc" ? DemosFilterModel.OrderBy.NameAsc : DemosFilterModel.OrderBy.NameDesc;
                        break;
                    case "date":
                        filterModel.Order = searchOrder == "asc" ? DemosFilterModel.OrderBy.DateAsc : DemosFilterModel.OrderBy.DateDesc;
                        break;
                    case "uploadedBy":
                        filterModel.Order = searchOrder == "asc" ? DemosFilterModel.OrderBy.UploadedByAsc : DemosFilterModel.OrderBy.UploadedByDesc;
                        break;
                }
            }

            var demoDtos = await _demosRepository.GetDemos(filterModel);

            var portalDemoEntries = new List<PortalDemoDto>();
            foreach (var demoDto in demoDtos)
            {
                var canDeletePortalDemo = await _authorizationService.AuthorizeAsync(User, demoDto, AuthPolicies.DeleteDemo);

                var portalDemoDto = new PortalDemoDto(demoDto);

                if (canDeletePortalDemo.Succeeded)
                    portalDemoDto.ShowDeleteLink = true;

                portalDemoEntries.Add(portalDemoDto);
            }

            return Json(new
            {
                model.Draw,
                recordsTotal,
                recordsFiltered,
                data = portalDemoEntries
            });
        }

        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            var demoUrl = await _demosRepository.GetDemoUrl(id);

            return Redirect(demoUrl.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id, bool filterGame = false)
        {
            var demoDto = await _demosRepository.GetDemo(id);
            if (demoDto == null) return NotFound();

            var canDeleteDemo = await _authorizationService.AuthorizeAsync(User, demoDto, AuthPolicies.DeleteDemo);

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
            var demoDto = await _demosRepository.GetDemo(id);
            if (demoDto == null) return NotFound();

            var canDeleteDemo = await _authorizationService.AuthorizeAsync(User, demoDto, AuthPolicies.DeleteDemo);

            if (!canDeleteDemo.Succeeded)
                return Unauthorized();

            await _demosRepository.DeleteDemo(id);

            _logger.LogInformation(EventIds.DemoManager, "User {User} has deleted {DemoId} under {GameType}", User.Username(), demoDto.DemoId, demoDto.Game);
            this.AddAlertSuccess($"The demo {demoDto.Name} has been successfully deleted from {demoDto.Game}");

            if (filterGame)
                return RedirectToAction(nameof(GameIndex), new { id = demoDto.Game });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ClientDemoList()
        {
            if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                return Content("AuthError: No auth key provided in the request");

            var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                _logger.LogDebug(EventIds.DemoManager, "ClientDemoList - Auth key header supplied but invalid");
                return Content("AuthError: Auth key header supplied but invalid");
            }

            var userId = await _demoAuthRepository.GetUserId(authKey);
            if (userId == null) return Content("AuthError: Auth key supplied but invalid. Try re-entering the auth key on your client");

            var user = await _userManager.FindByIdAsync(userId);
            var claimsPrincipal = await _signInManager.ClaimsFactory.CreateAsync(user);

            var filterModel = new DemosFilterModel().ApplyAuth(claimsPrincipal, null);
            var demoDtos = await _demosRepository.GetDemos(filterModel);

            var demos = demoDtos.Select(demo => new
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
                return Content("AuthError: No auth key provided in the request");

            var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                _logger.LogDebug(EventIds.DemoManager, "ClientDemoList - Auth key header supplied but invalid");
                return Content("AuthError: Auth key header supplied but invalid");
            }

            var userId = await _demoAuthRepository.GetUserId(authKey);
            if (userId == null) return Content("AuthError: Auth key supplied but invalid. Try re-entering the auth key on your client");

            var user = await _userManager.FindByIdAsync(userId);

            if (file == null || file.Length == 0) return Content("You must provide a file to be uploaded");

            var whitelistedExtensions = new List<string> {".dm_1", ".dm_6"};

            if (!whitelistedExtensions.Any(ext => file.FileName.EndsWith(ext)))
                return Content("Invalid file type - this must be a demo file");

            var gameTypeHeader = Request.Headers["demo-manager-game-type"].ToString();
            Enum.TryParse(gameTypeHeader, out GameType gameType);

            var fileName = $"{Guid.NewGuid().ToString()}.{gameType.DemoExtension()}";
            var path = Path.Combine(Path.GetTempPath(), fileName);

            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var localDemo = new LocalDemo(path, gameType);

            var demoDto = new DemoDto
            {
                Game = gameType,
                Name = file.FileName,
                FileName = fileName,
                Date = localDemo.Date,
                Map = localDemo.Map,
                Mod = localDemo.Mod,
                GameType = localDemo.GameType,
                Server = localDemo.Server,
                Size = localDemo.Size,
                UserId = user.Id
            };

            await _demosRepository.CreateDemo(demoDto, path);
            _logger.LogInformation(EventIds.DemoManager, "User {Username} has uploaded a new demo {FileName}", user.UserName, demoDto.FileName);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ClientDownload(Guid id)
        {
            if (!Request.Headers.ContainsKey("demo-manager-auth-key"))
                return Content("AuthError: No auth key provided in the request");

            var authKey = Request.Headers["demo-manager-auth-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authKey))
            {
                _logger.LogDebug(EventIds.DemoManager, "ClientDemoList - Auth key header supplied but invalid");
                return Content("AuthError: Auth key header supplied but invalid");
            }

            var userId = await _demoAuthRepository.GetUserId(authKey);
            if (userId == null) return Content("AuthError: Auth key supplied but invalid. Try re-entering the auth key on your client");

            var demoUrl = await _demosRepository.GetDemoUrl(id);

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