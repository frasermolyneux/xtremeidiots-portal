using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports
{
    public class CloseReportDto
    {
        public CloseReportDto(Guid userProfileId, string closingComments)
        {
            AdminUserProfileId = userProfileId;
            AdminClosingComments = closingComments;
        }

        [JsonProperty]
        public Guid AdminUserProfileId { get; private set; }

        [JsonProperty]
        public string AdminClosingComments { get; private set; }
    }
}
