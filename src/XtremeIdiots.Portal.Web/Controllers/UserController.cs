using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
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
            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin };
            var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

            var userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfile(id);

            ViewData["GameServers"] = gameServersApiResponse.Result.Data.Items;
            ViewData["GameServersSelect"] = new SelectList(gameServersApiResponse.Result.Data.Items, "GameServerId", "Title");

            return View(userProfileDtoApiResponse.Result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> GetUsersAjax()
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var userProfileResponseDto = await repositoryApiClient.UserProfiles.V1.GetUserProfiles(model.Search?.Value, model.Start, model.Length, UserProfilesOrder.DisplayNameAsc);

            return Json(new
            {
                model.Draw,
                recordsTotal = userProfileResponseDto.Result.Data.TotalCount,
                recordsFiltered = userProfileResponseDto.Result.Data.FilteredCount,
                data = userProfileResponseDto.Result.Data.Items
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogUserOut(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Index));

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                this.AddAlertWarning($"Could not find user with XtremeIdiots ID '{id}', or there is no user logged in with that XtremeIdiots ID");
                return RedirectToAction(nameof(Index));
            }

            await _userManager.UpdateSecurityStampAsync(user);

            this.AddAlertSuccess($"User {user.UserName} has been force logged out (this may take up to 15 minutes)");
            _logger.LogInformation("User {User} have force logged out {TargetUser}", User.Username(), user.UserName);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserClaim(Guid id, string claimType, string claimValue)
        {
            var userProfileResponseDto = await repositoryApiClient.UserProfiles.V1.GetUserProfile(id);

            if (userProfileResponseDto.IsNotFound)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userProfileResponseDto.Result.Data.XtremeIdiotsForumId);

            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(Guid.Parse(claimValue));

            var canCreateUserClaim = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.CreateUserClaim);

            if (!canCreateUserClaim.Succeeded)
                return Unauthorized();

            if (!userProfileResponseDto.Result.Data.UserProfileClaims.Any(claim => claim.ClaimType == claimType && claim.ClaimValue == claimValue))
            {
                var createUserProfileClaimDto = new CreateUserProfileClaimDto(userProfileResponseDto.Result.Data.UserProfileId, claimType, claimValue, false);

                await repositoryApiClient.UserProfiles.V1.CreateUserProfileClaim(userProfileResponseDto.Result.Data.UserProfileId, new List<CreateUserProfileClaimDto> { createUserProfileClaimDto });

                this.AddAlertSuccess($"The {claimType} claim has been added to {user.UserName}");
                _logger.LogInformation("User {User} has added a {ClaimType} with {ClaimValue} to {TargetUser}", User.Username(), claimType, claimValue, user.UserName);
            }
            else
                this.AddAlertSuccess($"Nothing to do - {user.UserName} already has the {claimType} claim");

            return RedirectToAction(nameof(ManageProfile), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserClaim(Guid id, Guid claimId)
        {
            var userProfileResponseDto = await repositoryApiClient.UserProfiles.V1.GetUserProfile(id);

            if (userProfileResponseDto.IsNotFound)
                return NotFound();

            var claim = userProfileResponseDto.Result.Data.UserProfileClaims.SingleOrDefault(c => c.UserProfileClaimId == claimId);

            if (claim == null)
                return NotFound();

            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(Guid.Parse(claim.ClaimValue));

            // Allow for legacy claims to be deleted
            var canDeleteUserClaim = false;
            if (gameServerApiResponse.IsNotFound)
            {
                canDeleteUserClaim = true;
            }
            else
            {
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.DeleteUserClaim);
                canDeleteUserClaim = authorizationResult.Succeeded;
            }

            if (!canDeleteUserClaim)
                return Unauthorized();

            await repositoryApiClient.UserProfiles.V1.DeleteUserProfileClaim(id, claimId);

            var user = await _userManager.FindByIdAsync(userProfileResponseDto.Result.Data.XtremeIdiotsForumId);
            if (user != null)
                await _userManager.UpdateSecurityStampAsync(user);

            this.AddAlertSuccess($"User {userProfileResponseDto.Result.Data.DisplayName}'s claim has been removed (this may take up to 15 minutes)");
            _logger.LogInformation("User {User} has removed a claim from {TargetUser}", User.Username(), userProfileResponseDto.Result.Data.DisplayName);

            return RedirectToAction(nameof(ManageProfile), new { id });
        }
    }
}