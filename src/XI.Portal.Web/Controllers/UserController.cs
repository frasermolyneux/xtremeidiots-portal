using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Auth.Contract.Models;
using XI.Portal.Users.Models;
using XI.Portal.Users.Repository;
using XI.Portal.Web.Extensions;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessUsers)]
    public class UserController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<PortalIdentityUser> _userManager;
        private readonly IUsersRepository _usersRepository;

        public UserController(
            IUsersRepository usersRepository,
            UserManager<PortalIdentityUser> userManager,
            ILogger<UserController> logger,
            IAuthorizationService authorizationService,
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient)
        {
            _usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryTokenProvider = repositoryTokenProvider;
            this.repositoryApiClient = repositoryApiClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Permissions()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManageProfile(string id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();

            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin };
            var (gameTypes, serverIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServers = await repositoryApiClient.GameServers.GetGameServers(accessToken, gameTypes, serverIds, null, 0, 0, "BannerServerListPosition");

            var user = await _usersRepository.GetUser(id);

            ViewData["GameServers"] = gameServers;
            ViewData["GameServersSelect"] = new SelectList(gameServers, "ServerId", "Title");

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAjax()
        {
            var users = await _usersRepository.GetUsers();
            return Json(new
            {
                data = users
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogUserOut(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Index));

            var user = await _userManager.FindByIdAsync(id);
            await _userManager.UpdateSecurityStampAsync(user);

            this.AddAlertSuccess($"User {user.UserName} has been force logged out (this may take up to 15 minutes)");
            _logger.LogInformation("User {User} have force logged out {TargetUser}", User.Username(), user.UserName);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserClaim(string id, string claimType, string claimValue)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            var portalClaims = await _usersRepository.GetUserClaims(id);

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, Guid.Parse(claimValue));

            var canCreateUserClaim = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.CreateUserClaim);

            if (!canCreateUserClaim.Succeeded)
                return Unauthorized();

            if (!portalClaims.Any(claim => claim.ClaimType == claimType && claim.ClaimValue == claimValue))
            {
                await _usersRepository.UpdatePortalClaim(id, new PortalClaimDto
                {
                    ClaimType = claimType,
                    ClaimValue = claimValue
                });

                this.AddAlertSuccess($"The {claimType} claim has been added to {user.UserName}");
                _logger.LogInformation("User {User} has added a {ClaimType} with {ClaimValue} to {TargetUser}", User.Username(), claimType, claimValue, user.UserName);
            }
            else
            {
                this.AddAlertSuccess($"Nothing to do - {user.UserName} already has the {claimType} claim");
            }

            return RedirectToAction(nameof(ManageProfile), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserClaim(string id, string claimId)
        {
            if (id == null || claimId == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            var userClaims = await _usersRepository.GetUserClaims(id);
            var claim = userClaims.SingleOrDefault(c => c.RowKey == claimId);

            if (claim == null)
                return NotFound();

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, Guid.Parse(claim.ClaimValue));

            var canDeleteUserClaim = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.DeleteUserClaim);

            if (!canDeleteUserClaim.Succeeded)
                return Unauthorized();

            await _usersRepository.RemoveUserClaim(id, claimId);
            await _userManager.UpdateSecurityStampAsync(user);

            this.AddAlertSuccess($"User {user.UserName}'s claim has been removed (this may take up to 15 minutes)");
            _logger.LogInformation("User {User} has removed a claim from {TargetUser}", User.Username(), user.UserName);

            return RedirectToAction(nameof(ManageProfile), new { id });
        }
    }
}