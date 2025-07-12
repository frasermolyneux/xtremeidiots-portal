using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
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
            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.FtpCredentials, UserProfileClaimType.RconCredentials };
            var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            foreach (var gameServerDto in gameServersApiResponse.Result.Data.Items)
            {
                var canViewFtpCredential = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.GameServerId), AuthPolicies.ViewFtpCredential);

                if (!canViewFtpCredential.Succeeded)
                    gameServerDto.ClearFtpCredentials();

                var canViewRconCredential = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.GameServerId), AuthPolicies.ViewRconCredential);

                if (!canViewRconCredential.Succeeded)
                    gameServerDto.ClearRconCredentials();
            }

            return View(gameServersApiResponse.Result.Data.Items);
        }
    }
}