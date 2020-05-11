using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Web.Controllers
{
    public class BannersController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;

        public BannersController(IGameServersRepository gameServersRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
        }

        public IActionResult GameServersList()
        {
            return View();
        }

        public async Task<IActionResult> GetGameServers()
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition
            };

            var gameServerDtos = (await _gameServersRepository.GetGameServers(filterModel))
                .Where(s => s.ShowOnBannerServerList && !string.IsNullOrWhiteSpace(s.HtmlBanner))
                .ToList();

            var htmlBanners = gameServerDtos.Select(gs => gs.HtmlBanner).ToList();

            return Json(htmlBanners);
        }
    }
}