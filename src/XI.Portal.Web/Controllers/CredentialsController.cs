using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Credentials.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

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
            var filterModel = new GameServerFilterModel().ApplyAuthForCredentials(User);
            var gameServerDtos = await _gameServersRepository.GetGameServers(filterModel);

            return View(gameServerDtos);
        }
    }
}