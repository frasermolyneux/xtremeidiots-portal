using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Data.Auth;
using XI.Portal.Servers.Repository;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.Admin)]
    public class CredentialsController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;

        public CredentialsController(IGameServersRepository gameServersRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
        }

        public async Task<IActionResult> Index()
        {
            var servers = await _gameServersRepository.GetGameServers(User);
            return View(servers);
        }
    }
}