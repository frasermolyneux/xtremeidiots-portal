using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

using System.Security.Claims;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.XtremeIdiots
{
    public class XtremeIdiotsAuth : IXtremeIdiotsAuth
    {
        private readonly IInvisionApiClient _forumsClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly ILogger<XtremeIdiotsAuth> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public XtremeIdiotsAuth(
            ILogger<XtremeIdiotsAuth> logger,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IInvisionApiClient forumsClient,
            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _forumsClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string? redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties("XtremeIdiots", redirectUrl);
        }

        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync();
        }

        public async Task<XtremeIdiotsAuthResult> ProcessExternalLogin(ExternalLoginInfo info)
        {
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);

            _logger.LogDebug("User {Username} had a {SignInResult} sign in result", username, result.ToString());

            switch (result.ToString())
            {
                case "Succeeded":
                    await UpdateExistingUser(info);
                    return XtremeIdiotsAuthResult.Success;
                case "Locked":
                    return XtremeIdiotsAuthResult.Locked;
                case "Failed":
                    await RegisterNewUser(info);
                    return XtremeIdiotsAuthResult.Success;
                default:
                    return XtremeIdiotsAuthResult.Failed;
            }
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        private async Task UpdateExistingUser(ExternalLoginInfo info)
        {
            var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _forumsClient.Core.GetMember(id);

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            var userClaims = await _userManager.GetClaimsAsync(user);

            await _userManager.RemoveClaimsAsync(user, userClaims);

            var userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(member.Id.ToString());

            if (userProfileDtoApiResponse.IsNotFound)
            {
                _ = await repositoryApiClient.UserProfiles.V1.CreateUserProfile(new CreateUserProfileDto(member.Id.ToString(), member.Name, member.Email)
                {
                    Title = member.Title,
                    FormattedName = member.FormattedName,
                    PrimaryGroup = member.PrimaryGroup.Name,
                    PhotoUrl = member.PhotoUrl,
                    ProfileUrl = member.ProfileUrl.ToString(),
                    TimeZone = member.TimeZone
                });

                userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(member.Id.ToString());
            }

            var claims = userProfileDtoApiResponse.Result.UserProfileClaims.Select(upc => new Claim(upc.ClaimType, upc.ClaimValue)).ToList();
            await _userManager.AddClaimsAsync(user, claims);
            await _signInManager.SignInAsync(user, true);
            await _signInManager.RefreshSignInAsync(user);
        }

        private async Task RegisterNewUser(ExternalLoginInfo info)
        {
            var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var member = await _forumsClient.Core.GetMember(id);

            var user = new IdentityUser { Id = id, UserName = username, Email = email };
            var createUserResult = await _userManager.CreateAsync(user);
            if (createUserResult.Succeeded)
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    var userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(member.Id.ToString());

                    if (userProfileDtoApiResponse.IsNotFound)
                    {
                        _ = await repositoryApiClient.UserProfiles.V1.CreateUserProfile(new CreateUserProfileDto(member.Id.ToString(), member.Name, member.Email)
                        {
                            Title = member.Title,
                            FormattedName = member.FormattedName,
                            PrimaryGroup = member.PrimaryGroup.Name,
                            PhotoUrl = member.PhotoUrl,
                            ProfileUrl = member.ProfileUrl.ToString(),
                            TimeZone = member.TimeZone
                        });

                        userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.V1.GetUserProfileByXtremeIdiotsId(member.Id.ToString());
                    }

                    var claims = userProfileDtoApiResponse.Result.UserProfileClaims.Select(upc => new Claim(upc.ClaimType, upc.ClaimValue)).ToList();
                    await _userManager.AddClaimsAsync(user, claims);
                    await _signInManager.SignInAsync(user, true);
                    await _signInManager.RefreshSignInAsync(user);

                    _logger.LogDebug("User {Username} created a new account with {Email} email", username, email);
                }
            }
        }
    }
}