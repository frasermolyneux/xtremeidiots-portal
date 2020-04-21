using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.UserHasCredentials)]
    public class CredentialsController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;

        public CredentialsController(IGameServersRepository gameServersRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
        }

        public async Task<IActionResult> Index()
        {
            var servers = await _gameServersRepository.GetGameServers(User, new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.FtpCredentials, PortalClaimTypes.RconCredentials});
            return View(servers);
        }
    }
}