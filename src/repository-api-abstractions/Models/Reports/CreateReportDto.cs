using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports
{
    public class CreateReportDto
    {
        public CreateReportDto(Guid playerId, Guid userProfileId, string comments)
        {
            PlayerId = playerId;
            UserProfileId = userProfileId;
            Comments = comments;
        }

        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]
        public Guid UserProfileId { get; private set; }
        [JsonProperty]
        public Guid? ServerId { get; private set; }
        [JsonProperty]
        public string Comments { get; private set; }
    }
}
