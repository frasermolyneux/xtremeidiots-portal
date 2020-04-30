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
    [Authorize(Policy = XtremeIdiotsPolicy.AccessCredentials)]
    public class CredentialsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IGameServersRepository _gameServersRepository;

        public CredentialsController(
            IAuthorizationService authorizationService,
            IGameServersRepository gameServersRepository)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
        }

        public async Task<IActionResult> Index()
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuthForCredentials(User);
            var gameServerDtos = await _gameServersRepository.GetGameServers(filterModel);

            foreach (var gameServerDto in gameServerDtos)
            {
                var canViewFtpCredential = await _authorizationService.AuthorizeAsync(User, gameServerDto, XtremeIdiotsPolicy.ViewFtpCredential);

                if (!canViewFtpCredential.Succeeded)
                {
                    gameServerDto.FtpHostname = string.Empty;
                    gameServerDto.FtpUsername = string.Empty;
                    gameServerDto.FtpPassword = string.Empty;
                }

                var canViewRconCredential = await _authorizationService.AuthorizeAsync(User, gameServerDto, XtremeIdiotsPolicy.ViewRconCredential);

                if (!canViewRconCredential.Succeeded)
                    gameServerDto.RconPassword = string.Empty;
            }

            return View(gameServerDtos);
        }
    }
}