using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Servers.Interfaces;

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
            var gameServersBanners = await _gameServersRepository.GetGameServerBanners();
            return View(gameServersBanners);
        }
    }
}
