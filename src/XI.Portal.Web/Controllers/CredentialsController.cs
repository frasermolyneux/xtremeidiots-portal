using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessCredentials)]
    public class CredentialsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

        public CredentialsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.FtpCredentials, PortalClaimTypes.RconCredentials };
            var (gameTypes, serverIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, serverIds, null, 0, 0, GameServerOrder.BannerServerListPosition);

            foreach (var gameServerDto in gameServerDtos)
            {
                var canViewFtpCredential = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.Id), AuthPolicies.ViewFtpCredential);

                if (!canViewFtpCredential.Succeeded)
                {
                    gameServerDto.FtpHostname = string.Empty;
                    gameServerDto.FtpUsername = string.Empty;
                    gameServerDto.FtpPassword = string.Empty;
                }

                var canViewRconCredential = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.Id), AuthPolicies.ViewRconCredential);

                if (!canViewRconCredential.Succeeded)
                    gameServerDto.RconPassword = string.Empty;
            }

            return View(gameServerDtos);
        }
    }
}