using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Forums.Interfaces;
using XI.Forums.Models;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Models;
using XI.Portal.Users.Repository;

namespace XI.Portal.Auth.XtremeIdiots
{
    public class XtremeIdiotsAuth : IXtremeIdiotsAuth
    {
        private readonly IForumsClient _forumsClient;
        private readonly ILogger<XtremeIdiotsAuth> _logger;
        private readonly SignInManager<PortalIdentityUser> _signInManager;
        private readonly UserManager<PortalIdentityUser> _userManager;
        private readonly IUsersRepository _usersRepository;

        public XtremeIdiotsAuth(
            ILogger<XtremeIdiotsAuth> logger,
            SignInManager<PortalIdentityUser> signInManager,
            UserManager<PortalIdentityUser> userManager,
            IUsersRepository usersRepository,
            IForumsClient forumsClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
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
            await AddPortalClaims(user);

            await _signInManager.SignInAsync(user, true);
        }

        private async Task RegisterNewUser(ExternalLoginInfo info)
        {
            var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var member = await _forumsClient.GetMember(id);

            var user = new PortalIdentityUser {Id = id, UserName = username, Email = email};
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

        private async Task AddPortalClaims(PortalIdentityUser identityUser)
        {
            var portalClaims = await _usersRepository.GetUserClaims(identityUser.Id);
            var claims = new List<Claim>();

            foreach (var claim in portalClaims) claims.Add(new Claim(claim.ClaimType, claim.ClaimValue));

            await _userManager.AddClaimsAsync(identityUser, claims);
        }

        private async Task AddXtremeIdiotsClaims(PortalIdentityUser identityUser, Member member)
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
                new Claim(XtremeIdiotsClaimTypes.PhotoUrl, member.PhotoUrl)
            };

            claims = claims.Concat(GetClaimsForGroup(member.PrimaryGroup)).ToList();
            claims = member.SecondaryGroups.Aggregate(claims, (current, group) => current.Concat(GetClaimsForGroup(group)).ToList());

            return claims;
        }

        private static IEnumerable<Claim> GetClaimsForGroup(Group group)
        {
            var claims = new List<Claim>();

            var groupName = group.Name.Replace("+", "").Trim();
            switch (groupName)
            {
                // Senior Admin
                case "Senior Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.SeniorAdmin, GameType.Unknown.ToString()));
                    break;

                // COD2
                case "COD2 Head Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.CallOfDuty2.ToString()));
                    break;
                case "COD2 Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.CallOfDuty2.ToString()));
                    break;
                case "COD2 Moderator":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.CallOfDuty2.ToString()));
                    break;

                //COD4
                case "COD4 Head Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.CallOfDuty4.ToString()));
                    break;
                case "COD4 Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.CallOfDuty4.ToString()));
                    break;
                case "COD4 Moderator":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.CallOfDuty4.ToString()));
                    break;

                //COD5
                case "COD5 Head Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.CallOfDuty5.ToString()));
                    break;
                case "COD5 Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.CallOfDuty5.ToString()));
                    break;
                case "COD5 Moderator":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.CallOfDuty5.ToString()));
                    break;

                //Insurgency
                case "Insurgency Head Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.Insurgency.ToString()));
                    break;
                case "Insurgency Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.Insurgency.ToString()));
                    break;
                case "Insurgency Moderator":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.Insurgency.ToString()));
                    break;

                //Minecraft
                case "Minecraft Head Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.Minecraft.ToString()));
                    break;
                case "Minecraft Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.Minecraft.ToString()));
                    break;
                case "Minecraft Moderator":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.Minecraft.ToString()));
                    break;

                //ARMA
                case "ARMA Head Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.ARMA.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.ARMA2.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.ARMA3.ToString()));
                    break;
                case "ARMA Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.ARMA.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.ARMA2.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.ARMA3.ToString()));
                    break;
                case "ARMA Moderator":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.ARMA.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.ARMA2.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.ARMA3.ToString()));
                    break;

                //Battlefield
                case "Battlefield Head Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.Battlefield1.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.Battlefield3.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.Battlefield4.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.Battlefield5.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.HeadAdmin, GameType.BattlefieldBadCompany2.ToString()));
                    break;
                case "Battlefield Admin":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.Battlefield1.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.Battlefield3.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.Battlefield4.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.Battlefield5.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.GameAdmin, GameType.BattlefieldBadCompany2.ToString()));
                    break;
                case "Battlefield Moderator":
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.Battlefield1.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.Battlefield3.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.Battlefield4.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.Battlefield5.ToString()));
                    claims.Add(new Claim(XtremeIdiotsClaimTypes.Moderator, GameType.BattlefieldBadCompany2.ToString()));
                    break;
            }

            return claims;
        }
    }
}