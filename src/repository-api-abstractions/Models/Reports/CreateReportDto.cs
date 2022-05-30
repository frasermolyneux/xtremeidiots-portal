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

        public Guid PlayerId { get; private set; }
        public Guid UserProfileId { get; private set; }
        public Guid? ServerId { get; private set; }
        public string Comments { get; private set; }
    }
}
