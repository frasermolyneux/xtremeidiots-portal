using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Web.ViewComponents
{
    public class GameServerListViewComponent : ViewComponent
    {
        private readonly IGameServersRepository _gameServersRepository;

        public GameServerListViewComponent(IGameServersRepository gameServersRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition
            };

            var gameServerDtos = (await _gameServersRepository.GetGameServers(filterModel))
                .Where(s => s.ShowOnBannerServerList && !string.IsNullOrWhiteSpace(s.HtmlBanner))
                .ToList();

            return View(gameServerDtos);
        }
    }
}