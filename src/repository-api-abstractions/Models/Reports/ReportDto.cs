using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports
{
    public class ReportDto
    {
        public Guid Id { get; internal set; }
        public Guid? PlayerId { get; internal set; }
        public Guid? UserProfileId { get; internal set; }
        public Guid? ServerId { get; internal set; }
        public GameType GameType { get; internal set; }
        public string? Comments { get; internal set; }
        public DateTime Timestamp { get; internal set; }
        public Guid? AdminUserProfileId { get; internal set; }
        public string? AdminClosingComments { get; internal set; }
        public bool Closed { get; internal set; }
        public DateTime? ClosedTimestamp { get; internal set; }

        public UserProfileDto? UserProfile { get; internal set; }
        public UserProfileDto? AdminUserProfile { get; internal set; }
    }
}
