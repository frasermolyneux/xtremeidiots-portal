using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessGameServers)]
    public class GameServersController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly ILogger<GameServersController> _logger;

        public GameServersController(
            ILogger<GameServersController> logger,
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, PortalClaimTypes.GameServer };
            var (gameTypes, serverIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, serverIds, null, 0, 0, GameServerOrder.BannerServerListPosition);

            var viewModels = gameServerDtos.Select(gs => gs.ToViewModel()).ToList();

            return View(viewModels);
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

#pragma warning disable CS8604 // Possible null reference argument. // ModelState check is just above.
            var gameServerDto = new CreateGameServerDto(model.Title, model.GameType, model.Hostname, model.QueryPort);
#pragma warning restore CS8604 // Possible null reference argument.
            var canCreateGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.CreateGameServer);

            if (!canCreateGameServer.Succeeded)
                return Unauthorized();

            gameServerDto.Title = model.Title;
            gameServerDto.Hostname = model.Hostname;
            gameServerDto.QueryPort = model.QueryPort;

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.EditGameServerFtp);

            if (canEditGameServerFtp.Succeeded)
            {
                gameServerDto.FtpHostname = model.FtpHostname;
                gameServerDto.FtpPort = model.FtpPort;
                gameServerDto.FtpUsername = model.FtpUsername;
                gameServerDto.FtpPassword = model.FtpPassword;
            }

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.EditGameServerRcon);

            if (canEditGameServerRcon.Succeeded)
                gameServerDto.RconPassword = model.RconPassword;

            gameServerDto.LiveStatusEnabled = model.LiveStatusEnabled;
            gameServerDto.ShowOnBannerServerList = model.ShowOnBannerServerList;
            gameServerDto.BannerServerListPosition = model.BannerServerListPosition;
            gameServerDto.HtmlBanner = model.HtmlBanner;
            gameServerDto.ShowOnPortalServerList = model.ShowOnPortalServerList;
            gameServerDto.ShowChatLog = model.ShowChatLog;

            await repositoryApiClient.GameServers.CreateGameServer(gameServerDto);

            _logger.LogInformation("User {User} has created a new game server for {GameType}", User.Username(), model.GameType);
            this.AddAlertSuccess($"The game server has been successfully created for {model.GameType}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerDto == null) return NotFound();

            var canViewGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewGameServer);

            if (!canViewGameServer.Succeeded)
                return Unauthorized();

            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            var banFileMonitorDtos = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(gameTypes, banFileMonitorIds, id, 0, 0, "BannerServerListPosition");

            var viewModels = new List<BanFileMonitorViewModel>();
            foreach (var banFileMonitor in banFileMonitorDtos)
                viewModels.Add(new BanFileMonitorViewModel
                {
                    BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                    FilePath = banFileMonitor.FilePath,
                    RemoteFileSize = banFileMonitor.RemoteFileSize,
                    LastSync = banFileMonitor.LastSync,
                    ServerId = banFileMonitor.ServerId,
                    GameServer = gameServerDto
                });

            var model = new GameServerDetailsViewModel
            {
                GameServerViewModel = gameServerDto.ToViewModel(),
                BanFileMonitors = viewModels
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerDto == null) return NotFound();

            AddGameTypeViewData(gameServerDto.GameType);

            var canEditGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.EditGameServer);

            if (!canEditGameServer.Succeeded)
                return Unauthorized();

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.EditGameServerFtp);

            if (!canEditGameServerFtp.Succeeded)
            {
                gameServerDto.FtpHostname = string.Empty;
                gameServerDto.FtpPort = 21;
                gameServerDto.FtpUsername = string.Empty;
                gameServerDto.FtpPassword = string.Empty;
            }

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.EditGameServerRcon);

            if (!canEditGameServerRcon.Succeeded)
                gameServerDto.RconPassword = string.Empty;

            return View(gameServerDto.ToViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameServerViewModel model)
        {

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(model.ServerId);

            if (gameServerDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                AddGameTypeViewData(model.GameType);
                return View(model);
            }

            var canEditGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.EditGameServer);

            if (!canEditGameServer.Succeeded)
                return Unauthorized();

            gameServerDto.Title = model.Title;
            gameServerDto.Hostname = model.Hostname;
            gameServerDto.QueryPort = model.QueryPort;

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.EditGameServerFtp);

            if (canEditGameServerFtp.Succeeded)
            {
                gameServerDto.FtpHostname = model.FtpHostname;
                gameServerDto.FtpPort = model.FtpPort;
                gameServerDto.FtpUsername = model.FtpUsername;
                gameServerDto.FtpPassword = model.FtpPassword;
            }

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.EditGameServerRcon);

            if (canEditGameServerRcon.Succeeded)
                gameServerDto.RconPassword = model.RconPassword;

            gameServerDto.LiveStatusEnabled = model.LiveStatusEnabled;
            gameServerDto.ShowOnBannerServerList = model.ShowOnBannerServerList;
            gameServerDto.BannerServerListPosition = model.BannerServerListPosition;
            gameServerDto.HtmlBanner = model.HtmlBanner;
            gameServerDto.ShowOnPortalServerList = model.ShowOnPortalServerList;
            gameServerDto.ShowChatLog = model.ShowChatLog;

            await repositoryApiClient.GameServers.UpdateGameServer(gameServerDto);

            _logger.LogInformation("User {User} has updated {GameServerId} under {GameType}", User.Username(), gameServerDto.Id, gameServerDto.GameType);
            this.AddAlertSuccess($"The game server {gameServerDto.Title} has been updated for {gameServerDto.GameType}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerDto == null) return NotFound();

            var canDeleteGameServer = await _authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);

            if (!canDeleteGameServer.Succeeded)
                return Unauthorized();

            return View(gameServerDto.ToViewModel());
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerDto == null) return NotFound();

            var canDeleteGameServer = await _authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);

            if (!canDeleteGameServer.Succeeded)
                return Unauthorized();

            await repositoryApiClient.GameServers.DeleteGameServer(id);

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