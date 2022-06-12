using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
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

            var gameServersApiResponse = await repositoryApiClient.GameServers.GetGameServers(gameTypes, serverIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            foreach (var gameServerDto in gameServersApiResponse.Result.Entries)
            {
                var canViewFtpCredential = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.Id), AuthPolicies.ViewFtpCredential);

                if (!canViewFtpCredential.Succeeded)
                    gameServerDto.ClearFtpCredentials();

                var canViewRconCredential = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.Id), AuthPolicies.ViewRconCredential);

                if (!canViewRconCredential.Succeeded)
                    gameServerDto.ClearRconCredentials();
            }

            return View(gameServersApiResponse.Result.Entries);
        }
    }
}