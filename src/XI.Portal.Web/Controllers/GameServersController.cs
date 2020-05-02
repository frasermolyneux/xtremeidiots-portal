using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Auth.GameServers.Extensions;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Portal.Web.Extensions;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessGameServers)]
    public class GameServersController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<GameServersController> _logger;

        public GameServersController(
            ILogger<GameServersController> logger,
            IAuthorizationService authorizationService,
            IGameServersRepository gameServersRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuthForGameServers(User);

            var gameServerDtos = await _gameServersRepository.GetGameServers(filterModel);

            return View(gameServerDtos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            AddGameTypeViewData();
            return View(new GameServerDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameServerDto model)
        {
            if (!ModelState.IsValid)
            {
                AddGameTypeViewData(model.GameType);
                return View(model);
            }

            var gameServerDto = new GameServerDto().ForGameType(model.GameType);
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

            await _gameServersRepository.UpdateGameServer(gameServerDto);

            _logger.LogInformation(EventIds.Management, "User {User} has created a new game server for {GameType}", User.Username(), model.GameType);
            this.AddAlertSuccess($"The game server has been successfully created for {model.GameType}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(id);
            if (gameServerDto == null) return NotFound();

            var canEditGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.ViewGameServer);

            if (!canEditGameServer.Succeeded)
                return Unauthorized();

            return View(gameServerDto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(id);
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
        public async Task<IActionResult> Edit(GameServerDto model)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(model.ServerId);
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

            await _gameServersRepository.UpdateGameServer(gameServerDto);

            _logger.LogInformation(EventIds.Management, "User {User} has updated {GameServerId} under {GameType}", User.Username(), gameServerDto.ServerId, gameServerDto.GameType);
            this.AddAlertSuccess($"The game server {gameServerDto.Title} has been updated for {gameServerDto.GameType}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(id);
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
            var gameServerDto = await _gameServersRepository.GetGameServer(id);
            if (gameServerDto == null) return NotFound();

            var canDeleteGameServer = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.DeleteGameServer);

            if (!canDeleteGameServer.Succeeded)
                return Unauthorized();

            await _gameServersRepository.DeleteGameServer(id);

            _logger.LogInformation(EventIds.Management, "User {User} has deleted {GameServerId} under {GameType}", User.Username(), gameServerDto.ServerId, gameServerDto.GameType);
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