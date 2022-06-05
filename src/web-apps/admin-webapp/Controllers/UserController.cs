﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Newtonsoft.Json;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
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

            var gameServersApiResponse = await repositoryApiClient.GameServers.GetGameServers(gameTypes, serverIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

            var userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.GetUserProfile(id);
            var userProfileViewModel = new UserProfileViewModel(userProfileDtoApiResponse.Result);

            ViewData["GameServers"] = gameServersApiResponse;
            ViewData["GameServersSelect"] = new SelectList(gameServersApiResponse.Result.Entries, "Id", "Title");

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

            var userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfiles(model.Search?.Value, model.Start, model.Length, UserProfilesOrder.DisplayNameAsc);

            return Json(new
            {
                model.Draw,
                recordsTotal = userProfileResponseDto.Result.TotalRecords,
                recordsFiltered = userProfileResponseDto.Result.FilteredRecords,
                data = userProfileResponseDto.Result.Entries
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
            var userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfile(id);

            if (userProfileResponseDto.IsNotFound)
                return NotFound();

            var user = await _userManager.FindByIdAsync(userProfileResponseDto.Result.XtremeIdiotsForumId);

            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(Guid.Parse(claimValue));

            var canCreateUserClaim = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.CreateUserClaim);

            if (!canCreateUserClaim.Succeeded)
                return Unauthorized();

            if (!userProfileResponseDto.Result.UserProfileClaimDtos.Any(claim => claim.ClaimType == claimType && claim.ClaimValue == claimValue))
            {
                var createUserProfileClaimDto = new CreateUserProfileClaimDto(userProfileResponseDto.Result.Id, claimType, claimValue, false);

                await repositoryApiClient.UserProfiles.CreateUserProfileClaim(userProfileResponseDto.Result.Id, new List<CreateUserProfileClaimDto> { createUserProfileClaimDto });

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
            var userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfile(id);

            if (userProfileResponseDto.IsNotFound)
                return NotFound();

            var claim = userProfileResponseDto.Result.UserProfileClaimDtos.SingleOrDefault(c => c.Id == claimId);

            if (claim == null)
                return NotFound();

            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(Guid.Parse(claim.ClaimValue));

            var canDeleteUserClaim = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.DeleteUserClaim);

            if (!canDeleteUserClaim.Succeeded)
                return Unauthorized();

            await repositoryApiClient.UserProfiles.DeleteUserProfileClaim(id, claimId);

            var user = await _userManager.FindByIdAsync(userProfileResponseDto.Result.XtremeIdiotsForumId);
            if (user != null)
                await _userManager.UpdateSecurityStampAsync(user);

            this.AddAlertSuccess($"User {userProfileResponseDto.Result.DisplayName}'s claim has been removed (this may take up to 15 minutes)");
            _logger.LogInformation("User {User} has removed a claim from {TargetUser}", User.Username(), userProfileResponseDto.Result.DisplayName);

            return RedirectToAction(nameof(ManageProfile), new { id });
        }
    }
}