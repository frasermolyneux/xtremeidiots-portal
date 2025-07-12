using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
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
            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameServer };
            var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return View(gameServersApiResponse.Result.Data.Items);
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
            var createGameServerDto = new CreateGameServerDto(model.Title, model.GameType, model.Hostname, model.QueryPort);
#pragma warning restore CS8604 // Possible null reference argument.
            var canCreateGameServer = await _authorizationService.AuthorizeAsync(User, createGameServerDto.GameType, AuthPolicies.CreateGameServer);

            if (!canCreateGameServer.Succeeded)
                return Unauthorized();

            createGameServerDto.Title = model.Title;
            createGameServerDto.Hostname = model.Hostname;
            createGameServerDto.QueryPort = model.QueryPort;

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, createGameServerDto.GameType, AuthPolicies.EditGameServerFtp);

            if (canEditGameServerFtp.Succeeded)
            {
                createGameServerDto.FtpHostname = model.FtpHostname;
                createGameServerDto.FtpPort = model.FtpPort;
                createGameServerDto.FtpUsername = model.FtpUsername;
                createGameServerDto.FtpPassword = model.FtpPassword;
            }

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, createGameServerDto.GameType, AuthPolicies.EditGameServerRcon);

            if (canEditGameServerRcon.Succeeded)
                createGameServerDto.RconPassword = model.RconPassword;

            createGameServerDto.LiveTrackingEnabled = model.LiveTrackingEnabled;
            createGameServerDto.BannerServerListEnabled = model.BannerServerListEnabled;
            createGameServerDto.ServerListPosition = model.ServerListPosition;
            createGameServerDto.HtmlBanner = model.HtmlBanner;
            createGameServerDto.PortalServerListEnabled = model.PortalServerListEnabled;
            createGameServerDto.ChatLogEnabled = model.ChatLogEnabled;
            createGameServerDto.BotEnabled = model.BotEnabled;

            await repositoryApiClient.GameServers.V1.CreateGameServer(createGameServerDto);

            _logger.LogInformation("User {User} has created a new game server for {GameType}", User.Username(), model.GameType);
            this.AddAlertSuccess($"The game server has been successfully created for {model.GameType}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewGameServer = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewGameServer);

            if (!canViewGameServer.Succeeded)
                return Unauthorized();

            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            gameServerApiResponse.Result.Data.ClearNoPermissionBanFileMonitors(gameTypes, banFileMonitorIds);

            return View(gameServerApiResponse.Result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            AddGameTypeViewData(gameServerApiResponse.Result.Data.GameType);

            var canEditGameServer = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.EditGameServer);

            if (!canEditGameServer.Succeeded)
                return Unauthorized();

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.EditGameServerFtp);

            if (!canEditGameServerFtp.Succeeded)
                gameServerApiResponse.Result.Data.ClearFtpCredentials();

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.EditGameServerRcon);

            if (!canEditGameServerRcon.Succeeded)
                gameServerApiResponse.Result.Data.ClearRconCredentials();

            return View(gameServerApiResponse.Result.Data.ToViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameServerViewModel model)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                AddGameTypeViewData(model.GameType);
                return View(model);
            }

            var canEditGameServer = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.EditGameServer);

            if (!canEditGameServer.Succeeded)
                return Unauthorized();

            var editGameServerDto = new EditGameServerDto(gameServerApiResponse.Result.Data.GameServerId);

            editGameServerDto.Title = model.Title;
            editGameServerDto.Hostname = model.Hostname;
            editGameServerDto.QueryPort = model.QueryPort;

            var canEditGameServerFtp = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.EditGameServerFtp);

            if (canEditGameServerFtp.Succeeded)
            {
                editGameServerDto.FtpHostname = model.FtpHostname;
                editGameServerDto.FtpPort = model.FtpPort;
                editGameServerDto.FtpUsername = model.FtpUsername;
                editGameServerDto.FtpPassword = model.FtpPassword;
            }

            var canEditGameServerRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.EditGameServerRcon);

            if (canEditGameServerRcon.Succeeded)
                editGameServerDto.RconPassword = model.RconPassword;

            editGameServerDto.LiveTrackingEnabled = model.LiveTrackingEnabled;
            editGameServerDto.BannerServerListEnabled = model.BannerServerListEnabled;
            editGameServerDto.ServerListPosition = model.ServerListPosition;
            editGameServerDto.HtmlBanner = model.HtmlBanner;
            editGameServerDto.PortalServerListEnabled = model.PortalServerListEnabled;
            editGameServerDto.ChatLogEnabled = model.ChatLogEnabled;
            editGameServerDto.BotEnabled = model.BotEnabled;

            await repositoryApiClient.GameServers.V1.UpdateGameServer(editGameServerDto);

            _logger.LogInformation("User {User} has updated {GameServerId} under {GameType}", User.Username(), gameServerApiResponse.Result.Data.GameServerId, gameServerApiResponse.Result.Data.GameType);
            this.AddAlertSuccess($"The game server {gameServerApiResponse.Result.Data.Title} has been updated for {gameServerApiResponse.Result.Data.GameType}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canDeleteGameServer = await _authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);

            if (!canDeleteGameServer.Succeeded)
                return Unauthorized();

            return View(gameServerApiResponse.Result.Data.ToViewModel());
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canDeleteGameServer = await _authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);

            if (!canDeleteGameServer.Succeeded)
                return Unauthorized();

            await repositoryApiClient.GameServers.V1.DeleteGameServer(id);

            _logger.LogInformation("User {User} has deleted {GameServerId} under {GameType}", User.Username(), gameServerApiResponse.Result.Data.GameServerId, gameServerApiResponse.Result.Data.GameType);
            this.AddAlertSuccess($"The game server {gameServerApiResponse.Result.Data.Title} has been deleted for {gameServerApiResponse.Result.Data.GameType}");

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