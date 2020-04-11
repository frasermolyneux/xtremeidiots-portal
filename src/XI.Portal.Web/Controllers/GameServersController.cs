using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Repository;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.Management)]
    public class GameServersController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<GameServersController> _logger;

        public GameServersController(IGameServersRepository gameServersRepository, ILogger<GameServersController> logger)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var servers = await _gameServersRepository.GetGameServers(User);
            return View(servers);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _gameServersRepository.GetGameServer(id, User);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["GameType"] = new SelectList(User.ClaimedGameTypes());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Title,GameType,Hostname,QueryPort,FtpHostname,FtpUsername,FtpPassword,RconPassword,ShowOnBannerServerList,BannerServerListPosition,HtmlBanner,ShowOnPortalServerList,ShowChatLog")]
            GameServers model)
        {
            if (!User.HasGameTypeClaim(model.GameType)) return Unauthorized();

            if (ModelState.IsValid)
            {
                await _gameServersRepository.CreateGameServer(model);

                _logger.LogInformation(EventIds.Management, "User {User} has created a new game server with Id {Id}", User.Username(), model.ServerId);

                TempData["Success"] = "A new Game Server has been successfully created";
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameType"] = new SelectList(User.ClaimedGameTypes());

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _gameServersRepository.GetGameServer(id, User);

            if (model == null) return NotFound();

            ViewData["GameType"] = new SelectList(User.ClaimedGameTypes(), model.GameType);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind(
                "ServerId,Title,GameType,Hostname,QueryPort,FtpHostname,FtpUsername,FtpPassword,RconPassword,ShowOnBannerServerList,BannerServerListPosition,HtmlBanner,ShowOnPortalServerList,ShowChatLog")]
            GameServers model)
        {
            if (id != model.ServerId) return NotFound();

            if (ModelState.IsValid)
                try
                {
                    await _gameServersRepository.UpdateGameServer(id, model, User);

                    _logger.LogInformation(EventIds.Management, "User {User} has modified a game server with Id {Id}", User.Username(), id);

                    TempData["Success"] = "The Game Server has been successfully updated";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await GameServersExists(model.ServerId))
                        return NotFound();
                    throw;
                }

            ViewData["GameType"] = new SelectList(User.ClaimedGameTypes(), model.GameType);

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _gameServersRepository.GetGameServer(id, User);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _gameServersRepository.RemoveGameServer(id, User);

            _logger.LogInformation(EventIds.Management, "User {User} has deleted a game server with Id {Id}", User.Username(), id);

            TempData["Success"] = "The Game Server has been successfully deleted";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> GameServersExists(Guid id)
        {
            return await _gameServersRepository.GameServerExists(id, User);
        }
    }
}