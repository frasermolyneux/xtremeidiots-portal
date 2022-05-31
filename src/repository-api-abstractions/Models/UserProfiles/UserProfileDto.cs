namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string? IdentityOid { get; set; }
        public string? XtremeIdiotsForumId { get; set; }
        public string? DisplayName { get; set; }
        public string? Title { get; set; }
        public string? FormattedName { get; set; }
        public string? PrimaryGroup { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }
        public string? ProfileUrl { get; set; }
        public string? TimeZone { get; set; }
    }
}
