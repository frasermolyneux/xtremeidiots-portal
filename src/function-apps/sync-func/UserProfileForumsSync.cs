using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.InvisionApiClient;
using XtremeIdiots.Portal.InvisionApiClient.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.SyncFunc
{
    public class UserProfileForumsSync
    {
        private const int TakeEntries = 20;
        private readonly ILogger<UserProfileForumsSync> logger;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IInvisionApiClient invisionApiClient;

        public UserProfileForumsSync(
            ILogger<UserProfileForumsSync> logger,
            IRepositoryApiClient repositoryApiClient,
            IInvisionApiClient invisionApiClient)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
            this.invisionApiClient = invisionApiClient;
        }

        [FunctionName("UserProfileForumsSync")]
        public async Task RunUserProfileForumsSync([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            var skip = 0;
            var userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfiles(skip, TakeEntries, null);

            do
            {
                foreach (var userProfileDto in userProfileResponseDto.Entries)
                {
                    logger.LogInformation($"UserProfileSync for '{userProfileDto.DisplayName}' with XtremeIdiots ID '{userProfileDto.XtremeIdiotsForumId}'");

                    if (!string.IsNullOrWhiteSpace(userProfileDto.XtremeIdiotsForumId))
                    {
                        try
                        {
                            var member = await invisionApiClient.Core.GetMember(userProfileDto.XtremeIdiotsForumId);

                            if (member != null)
                            {
                                userProfileDto.DisplayName = member.Name;
                                userProfileDto.Title = member.Title;
                                userProfileDto.FormattedName = member.FormattedName;
                                userProfileDto.PrimaryGroup = member.PrimaryGroup.Name;
                                userProfileDto.Email = member.Email;
                                userProfileDto.PhotoUrl = member.PhotoUrl;
                                userProfileDto.ProfileUrl = member.ProfileUrl.ToString();
                                userProfileDto.TimeZone = member.TimeZone;

                                await repositoryApiClient.UserProfiles.UpdateUserProfile(userProfileDto);

                                var existingClaims = await repositoryApiClient.UserProfiles.GetUserProfileClaims(userProfileDto.Id);
                                var nonSystemGeneratedClaims = existingClaims.Where(upc => !upc.SystemGenerated).ToList();

                                var activeClaims = GetClaimsForMember(member);
                                var claimsToSave = activeClaims.Concat(nonSystemGeneratedClaims).ToList();

                                await repositoryApiClient.UserProfiles.CreateUserProfileClaims(userProfileDto.Id, claimsToSave);
                            }
                            else
                            {
                                await repositoryApiClient.UserProfiles.CreateUserProfileClaims(userProfileDto.Id, new List<UserProfileClaimDto>());
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Failed to sync forum profile for {userProfileDto.XtremeIdiotsForumId}");
                        }

                    }
                }

                skip += TakeEntries;
                userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfiles(skip, TakeEntries, null);
            } while (userProfileResponseDto.Entries.Count > 0);
        }

        private static List<UserProfileClaimDto> GetClaimsForMember(Member member)
        {
            var claims = new List<UserProfileClaimDto>
            {
                new UserProfileClaimDto
                {
                    ClaimType = UserProfileClaimType.XtremeIdiotsId,
                    ClaimValue = member.Id.ToString(),
                    SystemGenerated = true
                },
                new UserProfileClaimDto
                {
                    ClaimType = "Email",
                    ClaimValue = member.Email,
                    SystemGenerated = true
                },
                new UserProfileClaimDto
                {
                    ClaimType = UserProfileClaimType.PhotoUrl,
                    ClaimValue = member.PhotoUrl,
                    SystemGenerated = true
                },
                new UserProfileClaimDto
                {
                    ClaimType = UserProfileClaimType.TimeZone,
                    ClaimValue = member.TimeZone,
                    SystemGenerated = true
                }
            };

            claims = claims.Concat(GetClaimsForGroup(member.PrimaryGroup)).ToList();
            claims = member.SecondaryGroups.Aggregate(claims, (current, group) => current.Concat(GetClaimsForGroup(group)).ToList());

            return claims;
        }

        private static List<UserProfileClaimDto> GetClaimsForGroup(Group group)
        {
            var claims = new List<UserProfileClaimDto>();

            var groupName = group.Name.Replace("+", "").Trim();
            switch (groupName)
            {
                // Senior Admin
                case "Senior Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.SeniorAdmin, ClaimValue = GameType.Unknown.ToString(), SystemGenerated = true });
                    break;

                // COD2
                case "COD2 Head Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.CallOfDuty2.ToString(), SystemGenerated = true });
                    break;
                case "COD2 Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.CallOfDuty2.ToString(), SystemGenerated = true });
                    break;
                case "COD2 Moderator":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.CallOfDuty2.ToString(), SystemGenerated = true });
                    break;

                //COD4
                case "COD4 Head Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.CallOfDuty4.ToString(), SystemGenerated = true });
                    break;
                case "COD4 Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.CallOfDuty4.ToString(), SystemGenerated = true });
                    break;
                case "COD4 Moderator":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.CallOfDuty4.ToString(), SystemGenerated = true });
                    break;

                //COD5
                case "COD5 Head Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.CallOfDuty5.ToString(), SystemGenerated = true });
                    break;
                case "COD5 Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.CallOfDuty5.ToString(), SystemGenerated = true });
                    break;
                case "COD5 Moderator":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.CallOfDuty5.ToString(), SystemGenerated = true });
                    break;

                //Insurgency
                case "Insurgency Head Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Insurgency.ToString(), SystemGenerated = true });
                    break;
                case "Insurgency Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Insurgency.ToString(), SystemGenerated = true });
                    break;
                case "Insurgency Moderator":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Insurgency.ToString(), SystemGenerated = true });
                    break;

                //Minecraft
                case "Minecraft Head Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Minecraft.ToString(), SystemGenerated = true });
                    break;
                case "Minecraft Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Minecraft.ToString(), SystemGenerated = true });
                    break;
                case "Minecraft Moderator":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Minecraft.ToString(), SystemGenerated = true });
                    break;

                //ARMA
                case "ARMA Head Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Arma.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Arma2.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Arma3.ToString(), SystemGenerated = true });
                    break;
                case "ARMA Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Arma.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Arma2.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Arma3.ToString(), SystemGenerated = true });
                    break;
                case "ARMA Moderator":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Arma.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Arma2.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Arma3.ToString(), SystemGenerated = true });
                    break;

                //Battlefield
                case "Battlefield Head Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Battlefield1.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Battlefield3.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Battlefield4.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.Battlefield5.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.HeadAdmin, ClaimValue = GameType.BattlefieldBadCompany2.ToString(), SystemGenerated = true });
                    break;
                case "Battlefield Admin":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Battlefield1.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Battlefield3.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Battlefield4.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.Battlefield5.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.GameAdmin, ClaimValue = GameType.BattlefieldBadCompany2.ToString(), SystemGenerated = true });
                    break;
                case "Battlefield Moderator":
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Battlefield1.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Battlefield3.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Battlefield4.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.Battlefield5.ToString(), SystemGenerated = true });
                    claims.Add(new UserProfileClaimDto { ClaimType = UserProfileClaimType.Moderator, ClaimValue = GameType.BattlefieldBadCompany2.ToString(), SystemGenerated = true });
                    break;
            }

            return claims;
        }
    }
}
