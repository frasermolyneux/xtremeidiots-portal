using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Auth.GameServers.Extensions;
using XI.Portal.Web.Extensions;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessGameServers)]
    public class GameServersController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly ILogger<GameServersController> _logger;

        public GameServersController(
            ILogger<GameServersController> logger,
            IAuthorizationService authorizationService,
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryTokenProvider = repositoryTokenProvider;
            this.repositoryApiClient = repositoryApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();

            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, PortalClaimTypes.GameServer };
            var (gameTypes, serverIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(accessToken, gameTypes, serverIds, null, 0, 0, "BannerServerListPosition");

            return View(gameServerDtos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            AddGameTypeViewData();
            return View(new GameServerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameServerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                AddGameTypeViewData(model.GameType);
                return View(model);
            }

            var gameServerDto = new GameServerDto()
            {
                GameType = model.GameType
            };

            var canCreateGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.CreateGameServer);

            if (!canCreateGameServer.Succeeded)
                return Unauthorized();

            gameServerDto.Title = model.Title;
            gameServerDto.Hostname = model.Hostname;
            gameServerDto.QueryPort = model.QueryPort;

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.EditGameServerFtp);

            if (canEditGameServerFtp.Succeeded)
            {
                gameServerDto.FtpHostname = model.FtpHostname;
                gameServerDto.FtpUsername = model.FtpUsername;
                gameServerDto.FtpPassword = model.FtpPassword;
            }

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.EditGameServerRcon);

            if (canEditGameServerRcon.Succeeded)
                gameServerDto.RconPassword = model.RconPassword;

            gameServerDto.ShowOnBannerServerList = model.ShowOnBannerServerList;
            gameServerDto.BannerServerListPosition = model.BannerServerListPosition;
            gameServerDto.HtmlBanner = model.HtmlBanner;
            gameServerDto.ShowOnPortalServerList = model.ShowOnPortalServerList;
            gameServerDto.ShowChatLog = model.ShowChatLog;

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            await repositoryApiClient.GameServers.CreateGameServer(accessToken, gameServerDto);

            _logger.LogInformation("User {User} has created a new game server for {GameType}", User.Username(), model.GameType);
            this.AddAlertSuccess($"The game server has been successfully created for {model.GameType}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, id);

            if (gameServerDto == null) return NotFound();

            var canEditGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.ViewGameServer);

            if (!canEditGameServer.Succeeded)
                return Unauthorized();

            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            var banFileMonitorDtos = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(accessToken, gameTypes, banFileMonitorIds, id, 0, 0, "BannerServerListPosition");

            List<BanFileMonitorViewModel> viewModels = new List<BanFileMonitorViewModel>();
            foreach (var banFileMonitor in banFileMonitorDtos)
            {
                viewModels.Add(new BanFileMonitorViewModel
                {
                    BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                    FilePath = banFileMonitor.FilePath,
                    RemoteFileSize = banFileMonitor.RemoteFileSize,
                    LastSync = banFileMonitor.LastSync,
                    ServerId = banFileMonitor.ServerId,
                    GameServer = gameServerDto
                });
            }

            var model = new GameServerDetailsViewModel
            {
                GameServerDto = gameServerDto,
                BanFileMonitors = viewModels
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, id);

            if (gameServerDto == null) return NotFound();

            AddGameTypeViewData(gameServerDto.GameType);

            var canEditGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.EditGameServer);

            if (!canEditGameServer.Succeeded)
                return Unauthorized();

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.EditGameServerFtp);

            if (!canEditGameServerFtp.Succeeded)
            {
                gameServerDto.FtpHostname = string.Empty;
                gameServerDto.FtpUsername = string.Empty;
                gameServerDto.FtpPassword = string.Empty;
            }

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.EditGameServerRcon);

            if (!canEditGameServerRcon.Succeeded)
                gameServerDto.RconPassword = string.Empty;

            return View(gameServerDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameServerViewModel model)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, model.ServerId);

            if (gameServerDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                AddGameTypeViewData(model.GameType);
                return View(model);
            }

            var canEditGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.EditGameServer);

            if (!canEditGameServer.Succeeded)
                return Unauthorized();

            gameServerDto.Title = model.Title;
            gameServerDto.Hostname = model.Hostname;
            gameServerDto.QueryPort = model.QueryPort;

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.EditGameServerFtp);

            if (canEditGameServerFtp.Succeeded)
            {
                gameServerDto.FtpHostname = model.FtpHostname;
                gameServerDto.FtpUsername = model.FtpUsername;
                gameServerDto.FtpPassword = model.FtpPassword;
            }

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.EditGameServerRcon);

            if (canEditGameServerRcon.Succeeded)
                gameServerDto.RconPassword = model.RconPassword;

            gameServerDto.ShowOnBannerServerList = model.ShowOnBannerServerList;
            gameServerDto.BannerServerListPosition = model.BannerServerListPosition;
            gameServerDto.HtmlBanner = model.HtmlBanner;
            gameServerDto.ShowOnPortalServerList = model.ShowOnPortalServerList;
            gameServerDto.ShowChatLog = model.ShowChatLog;

            await repositoryApiClient.GameServers.UpdateGameServer(accessToken, gameServerDto);

            _logger.LogInformation("User {User} has updated {GameServerId} under {GameType}", User.Username(), gameServerDto.Id, gameServerDto.GameType);
            this.AddAlertSuccess($"The game server {gameServerDto.Title} has been updated for {gameServerDto.GameType}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, id);

            if (gameServerDto == null) return NotFound();

            var canDeleteGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.DeleteGameServer);

            if (!canDeleteGameServer.Succeeded)
                return Unauthorized();

            return View(gameServerDto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, id);

            if (gameServerDto == null) return NotFound();

            var canDeleteGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.DeleteGameServer);

            if (!canDeleteGameServer.Succeeded)
                return Unauthorized();

            await repositoryApiClient.GameServers.DeleteGameServer(accessToken, id);

            _logger.LogInformation("User {User} has deleted {GameServerId} under {GameType}", User.Username(), gameServerDto.Id, gameServerDto.GameType);
            this.AddAlertSuccess($"The game server {gameServerDto.Title} has been deleted for {gameServerDto.GameType}");

            return RedirectToAction(nameof(Index));
        }

        private void AddGameTypeViewData(GameType? selected = null)
        {
            if (selected == null)
                selected = GameType.Unknown;

            var gameTypes = User.GetGameTypesForGameServers();
            ViewData["GameType"] = new SelectList(gameTypes, selected);
        }
    }
}