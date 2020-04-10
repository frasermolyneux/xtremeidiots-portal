using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Forums.Client;
using XI.Forums.Models;
using XI.Portal.Web.Constants;
using XI.Portal.Web.Extensions;
using IdentityUser = ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityUser;

namespace XI.Portal.Web.Auth
{
    public class XtremeIdiotsAuth : IXtremeIdiotsAuth
    {
        private readonly IForumsClient _forumsClient;
        private readonly ILogger<XtremeIdiotsAuth> _logger;
        private readonly Microsoft.AspNetCore.Identity.SignInManager<IdentityUser> _signInManager;
        private readonly Microsoft.AspNetCore.Identity.UserManager<IdentityUser> _userManager;

        public XtremeIdiotsAuth(
            ILogger<XtremeIdiotsAuth> logger, Microsoft.AspNetCore.Identity.SignInManager<IdentityUser> signInManager, Microsoft.AspNetCore.Identity.UserManager<IdentityUser> userManager,
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

            _logger.LogDebug(EventIds.User, "User {Username} had a {SignInResult} sign in result", username, result.ToString());

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
            await AddXtremeIdiotsClaims(user, member);

            await _signInManager.SignInAsync(user, true);
        }

        private async Task RegisterNewUser(ExternalLoginInfo info)
        {
            var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var member = await _forumsClient.GetMember(id);

            var user = new IdentityUser {Id = id, UserName = username, Email = email};
            var createUserResult = await _userManager.CreateAsync(user);
            if (createUserResult.Succeeded)
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await AddXtremeIdiotsClaims(user, member);
                    await _signInManager.SignInAsync(user, true);
                    _logger.LogDebug(EventIds.User, "User {Username} created a new account with {Email} email", username, email);
                }
            }
        }

        private async Task AddXtremeIdiotsClaims(IdentityUser identityUser, Member member)
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
                new Claim(ClaimTypes.Email, member.Email),
                new Claim(XtremeIdiotsClaimTypes.PhotoUrl, member.PhotoUrl),
                new Claim(XtremeIdiotsClaimTypes.Group, primaryGroup)
            };

            foreach (var group in member.SecondaryGroups)
            {
                var groupName = group.Name.Replace("+", "").Trim();

                if (!claims.Any(claim => claim.Type == XtremeIdiotsClaimTypes.Group && claim.Value == groupName))
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Group, groupName));
            }

            var gameClaims = GetGameClaims(claims);
            claims = claims.Concat(gameClaims).ToList();

            return claims;
        }

        private static IEnumerable<Claim> GetGameClaims(IEnumerable<Claim> existingClaims)
        {
            var claims = new List<Claim>();

            foreach (var claim in existingClaims.Where(claim => claim.Type == XtremeIdiotsClaimTypes.Group))
            {
                if (claim.Value == "Senior Admin")
                    foreach (GameType gameType in Enum.GetValues(typeof(GameType)))
                        claims.AddGameClaimIfNotExists(gameType);

                if (claim.Value.Contains("COD2")) claims.AddGameClaimIfNotExists(GameType.CallOfDuty2);

                if (claim.Value.Contains("COD4")) claims.AddGameClaimIfNotExists(GameType.CallOfDuty4);

                if (claim.Value.Contains("COD5")) claims.AddGameClaimIfNotExists(GameType.CallOfDuty5);

                if (claim.Value.Contains("Insurgency")) claims.AddGameClaimIfNotExists(GameType.Insurgency);

                if (claim.Value.Contains("ARMA"))
                {
                    claims.AddGameClaimIfNotExists(GameType.ARMA);
                    claims.AddGameClaimIfNotExists(GameType.ARMA2);
                    claims.AddGameClaimIfNotExists(GameType.ARMA3);
                }

                if (claim.Value.Contains("Minecraft")) claims.AddGameClaimIfNotExists(GameType.Minecraft);

                if (claim.Value.Contains("Rust")) claims.AddGameClaimIfNotExists(GameType.Rust);
            }

            return claims;
        }
    }
}