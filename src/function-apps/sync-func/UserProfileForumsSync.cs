using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XtremeIdiots.Portal.InvisionApiClient;
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
            var userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfiles(skip, TakeEntries);

            do
            {
                foreach (var userProfileDto in userProfileResponseDto.Entries)
                {
                    logger.LogInformation($"UserProfileSync for '{userProfileDto.DisplayName}' with XtremeIdiots ID '{userProfileDto.XtremeIdiotsForumId}'");

                    if (!string.IsNullOrWhiteSpace(userProfileDto.XtremeIdiotsForumId))
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
                        }
                    }
                }

                skip += TakeEntries;
                userProfileResponseDto = await repositoryApiClient.UserProfiles.GetUserProfiles(skip, TakeEntries);
            } while (userProfileResponseDto.Entries.Count > 0);
        }
    }
}
