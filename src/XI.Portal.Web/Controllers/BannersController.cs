using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Web.Controllers
{
    public class BannersController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;

        public BannersController(IGameServersRepository gameServersRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
        }

        public async Task<IActionResult> GameServersList()
        {
            var gameServersBanners = await _gameServersRepository.GetGameServerBanners();
            return View(gameServersBanners);
        }
    }
}