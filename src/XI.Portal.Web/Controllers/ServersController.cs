using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [AllowAnonymous]
    public class ServersController : Controller
    {
        private readonly IGameServerStatusRepository _gameServerStatusRepository;

        public ServersController(IGameServerStatusRepository gameServerStatusRepository)
        {
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
        }

        public async Task<IActionResult> Index()
        {
            var servers = await _gameServerStatusRepository.GetAllStatusModels(null, null, TimeSpan.FromMinutes(-15));
            return View(servers);
        }
    }
}