using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Extensions;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessUsers)]
    public class UserController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(
            UserManager<IdentityUser> userManager,
            ILogger<UserController> logger,
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

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
        public async Task<IActionResult> ManageProfile(Guid id)
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin };
            var (gameTypes, serverIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServers = await repositoryApiClient.GameServers.GetGameServers(gameTypes, serverIds, null, 0, 0, GameServerOrder.BannerServerListPosition);

            var userProfileDto = await repositoryApiClient.UserProfiles.GetUserProfile(id);
            var userProfileClaimDtos = await repositoryApiClient.UserProfiles.GetUserProfileClaims(id);

            var userProfileViewModel = new UserProfileViewModel
            {
                UserProfile = userProfileDto,
                UserProfileClaims = userProfileClaimDtos
            };

            ViewData["GameServers"] = gameServers;
            ViewData["GameServersSelect"] = new SelectList(gameServers, "Id", "Title");

            return View(userProfileViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetUsersAjax()
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfiles(model.Start, model.Length, model.Search?.Value);

            return Json(new
            {
                model.Draw,
                recordsTotal = userProfileResponseDto.TotalRecords,
                recordsFiltered = userProfileResponseDto.FilteredRecords,
                data = userProfileResponseDto.Entries
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
        public async Task<IActionResult> CreateUserClaim(Guid id, string claimType, string claimValue)
        {
            var userProfileDto = await repositoryApiClient.UserProfiles.GetUserProfile(id);

            if (userProfileDto == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userProfileDto.XtremeIdiotsForumId);
            var userProfileClaimDtos = await repositoryApiClient.UserProfiles.GetUserProfileClaims(userProfileDto.Id);

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(Guid.Parse(claimValue));

            var canCreateUserClaim = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.CreateUserClaim);

            if (!canCreateUserClaim.Succeeded)
                return Unauthorized();

            if (!userProfileClaimDtos.Any(claim => claim.ClaimType == claimType && claim.ClaimValue == claimValue))
            {
                userProfileClaimDtos.Add(new UserProfileClaimDto
                {
                    ClaimType = claimType,
                    ClaimValue = claimValue,
                    SystemGenerated = false
                });

                await repositoryApiClient.UserProfiles.CreateUserProfileClaims(userProfileDto.Id, userProfileClaimDtos);

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
        public async Task<IActionResult> RemoveUserClaim(Guid id, Guid claimId)
        {
            var userProfileDto = await repositoryApiClient.UserProfiles.GetUserProfile(id);

            if (userProfileDto == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userProfileDto.XtremeIdiotsForumId);
            var userProfileClaimDtos = await repositoryApiClient.UserProfiles.GetUserProfileClaims(userProfileDto.Id);

            var claim = userProfileClaimDtos.SingleOrDefault(c => c.Id == claimId);

            if (claim == null)
                return NotFound();

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(Guid.Parse(claim.ClaimValue));

            var canDeleteUserClaim = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.DeleteUserClaim);

            if (!canDeleteUserClaim.Succeeded)
                return Unauthorized();

            userProfileClaimDtos.Remove(claim);
            await repositoryApiClient.UserProfiles.CreateUserProfileClaims(userProfileDto.Id, userProfileClaimDtos);

            await _userManager.UpdateSecurityStampAsync(user);

            this.AddAlertSuccess($"User {user.UserName}'s claim has been removed (this may take up to 15 minutes)");
            _logger.LogInformation("User {User} has removed a claim from {TargetUser}", User.Username(), user.UserName);

            return RedirectToAction(nameof(ManageProfile), new { id });
        }
    }
}