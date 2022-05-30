namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports
{
    public class CloseReportDto
    {
        public CloseReportDto(Guid userProfileId, string closingComments)
        {
            AdminUserProfileId = userProfileId;
            AdminClosingComments = closingComments;
        }

        public Guid AdminUserProfileId { get; private set; }
        public string AdminClosingComments { get; private set; }
    }
}
