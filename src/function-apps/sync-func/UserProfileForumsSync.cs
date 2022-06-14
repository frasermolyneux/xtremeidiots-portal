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
            var userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfiles(null, skip, TakeEntries, null);

            do
            {
                foreach (var userProfileDto in userProfileResponseDto.Result.Entries)
                {
                    logger.LogInformation($"UserProfileSync for '{userProfileDto.DisplayName}' with XtremeIdiots ID '{userProfileDto.XtremeIdiotsForumId}'");

                    if (!string.IsNullOrWhiteSpace(userProfileDto.XtremeIdiotsForumId))
                    {
                        try
                        {
                            var member = await invisionApiClient.Core.GetMember(userProfileDto.XtremeIdiotsForumId);

                            if (member != null)
                            {
                                var editUserProfileDto = new EditUserProfileDto(userProfileDto.UserProfileId)
                                {
                                    DisplayName = userProfileDto.DisplayName,
                                    FormattedName = userProfileDto.FormattedName,
                                    PrimaryGroup = userProfileDto.PrimaryGroup,
                                    Email = userProfileDto.Email,
                                    PhotoUrl = userProfileDto.PhotoUrl,
                                    ProfileUrl = userProfileDto.ProfileUrl,
                                    TimeZone = userProfileDto.TimeZone
                                };

                                await repositoryApiClient.UserProfiles.UpdateUserProfile(editUserProfileDto);

                                var nonSystemGeneratedClaims = userProfileDto.UserProfileClaims
                                    .Where(upc => !upc.SystemGenerated).Select(upc => new CreateUserProfileClaimDto(userProfileDto.UserProfileId, upc.ClaimType, upc.ClaimValue, upc.SystemGenerated))
                                    .ToList();

                                var activeClaims = GetClaimsForMember(userProfileDto.UserProfileId, member);
                                var claimsToSave = activeClaims.Concat(nonSystemGeneratedClaims).ToList();

                                await repositoryApiClient.UserProfiles.SetUserProfileClaims(userProfileDto.UserProfileId, claimsToSave);
                            }
                            else
                            {
                                await repositoryApiClient.UserProfiles.SetUserProfileClaims(userProfileDto.UserProfileId, new List<CreateUserProfileClaimDto>());
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Failed to sync forum profile for {userProfileDto.XtremeIdiotsForumId}");
                        }

                    }
                }

                skip += TakeEntries;
                userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfiles(null, skip, TakeEntries, null);
            } while (userProfileResponseDto.Result.Entries.Count > 0);
        }

        private static List<CreateUserProfileClaimDto> GetClaimsForMember(Guid userProfileId, Member member)
        {
            var claims = new List<CreateUserProfileClaimDto>
            {
                new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.XtremeIdiotsId, member.Id.ToString(), true),
                new CreateUserProfileClaimDto(userProfileId, "Email", member.Email, true),
                new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.PhotoUrl, member.PhotoUrl, true),
                new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.TimeZone, member.TimeZone, true),
            };

            claims = claims.Concat(GetClaimsForGroup(userProfileId, member.PrimaryGroup)).ToList();
            claims = member.SecondaryGroups.Aggregate(claims, (current, group) => current.Concat(GetClaimsForGroup(userProfileId, group)).ToList());

            return claims;
        }

        private static List<CreateUserProfileClaimDto> GetClaimsForGroup(Guid userProfileId, Group group)
        {
            var claims = new List<CreateUserProfileClaimDto>();

            var groupName = group.Name.Replace("+", "").Trim();
            switch (groupName)
            {
                // Senior Admin
                case "Senior Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.SeniorAdmin, GameType.Unknown.ToString(), true));
                    break;

                // COD2
                case "COD2 Head Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.CallOfDuty2.ToString(), true));
                    break;
                case "COD2 Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.CallOfDuty2.ToString(), true));
                    break;
                case "COD2 Moderator":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.CallOfDuty2.ToString(), true));
                    break;

                //COD4
                case "COD4 Head Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.CallOfDuty4.ToString(), true));
                    break;
                case "COD4 Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.CallOfDuty4.ToString(), true));
                    break;
                case "COD4 Moderator":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.CallOfDuty4.ToString(), true));
                    break;

                //COD5
                case "COD5 Head Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.CallOfDuty5.ToString(), true));
                    break;
                case "COD5 Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.CallOfDuty5.ToString(), true));
                    break;
                case "COD5 Moderator":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.CallOfDuty5.ToString(), true));
                    break;

                //Insurgency
                case "Insurgency Head Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Insurgency.ToString(), true));
                    break;
                case "Insurgency Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Insurgency.ToString(), true));
                    break;
                case "Insurgency Moderator":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Insurgency.ToString(), true));
                    break;

                //Minecraft
                case "Minecraft Head Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Minecraft.ToString(), true));
                    break;
                case "Minecraft Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Minecraft.ToString(), true));
                    break;
                case "Minecraft Moderator":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Minecraft.ToString(), true));
                    break;

                //ARMA
                case "ARMA Head Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Arma.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Arma2.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Arma3.ToString(), true));
                    break;
                case "ARMA Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Arma.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Arma2.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Arma3.ToString(), true));
                    break;
                case "ARMA Moderator":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Arma.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Arma2.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Arma3.ToString(), true));
                    break;

                //Battlefield
                case "Battlefield Head Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Battlefield1.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Battlefield3.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Battlefield4.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.Battlefield5.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.HeadAdmin, GameType.BattlefieldBadCompany2.ToString(), true));
                    break;
                case "Battlefield Admin":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Battlefield1.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Battlefield3.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Battlefield4.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.Battlefield5.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.GameAdmin, GameType.BattlefieldBadCompany2.ToString(), true));
                    break;
                case "Battlefield Moderator":
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Battlefield1.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Battlefield3.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Battlefield4.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.Battlefield5.ToString(), true));
                    claims.Add(new CreateUserProfileClaimDto(userProfileId, UserProfileClaimType.Moderator, GameType.BattlefieldBadCompany2.ToString(), true));
                    break;
            }

            return claims;
        }
    }
}
