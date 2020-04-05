using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using XI.Forums;
using XI.Forums.Models;
using XI.Portal.Web.Constants;
using IdentityUser = ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityUser;

namespace XI.Portal.Web.Auth
{
    public class XtremeIdiotsAuth : IXtremeIdiotsAuth
    {
        private readonly IForumsClient _forumsClient;
        private readonly ILogger<XtremeIdiotsAuth> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public XtremeIdiotsAuth(
            ILogger<XtremeIdiotsAuth> logger,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IForumsClient forumsClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _forumsClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string redirectUrl)
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
            var member = await _forumsClient.GetMember(id);

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            var userClaims = await _userManager.GetClaimsAsync(user);

            await _userManager.RemoveClaimsAsync(user, userClaims);
            await AddClaims(user, member);
        }

        private async Task RegisterNewUser(ExternalLoginInfo info)
        {
            var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var member = await _forumsClient.GetMember(id);

            var user = new IdentityUser {UserName = username, Email = email};
            var createUserResult = await _userManager.CreateAsync(user);
            if (createUserResult.Succeeded)
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await AddClaims(user, member);
                    await _signInManager.SignInAsync(user, true);
                    _logger.LogDebug("User {Username} created a new account with {Email} email", username, email);
                }
            }
        }

        private async Task AddClaims(IdentityUser identityUser, Member member)
        {
            var claims = GetClaimsForMember(member);
            await _userManager.AddClaimsAsync(identityUser, claims);
        }

        private static IEnumerable<Claim> GetClaimsForMember(Member member)
        {
            var primaryGroup = member.PrimaryGroup.Name.Replace("+", "").Trim();

            var claims = new List<Claim>
            {
                new Claim(XtremeIdiotsClaimTypes.XtremeIdiotsId, member.Id.ToString()),
                new Claim(XtremeIdiotsClaimTypes.Group, primaryGroup)
            };

            foreach (var group in member.SecondaryGroups)
            {
                var groupName = group.Name.Replace("+", "").Trim();

                if (!claims.Any(claim => claim.Type == XtremeIdiotsClaimTypes.Group && claim.Value == groupName))
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Group, groupName));
            }

            return claims;
        }
    }
}