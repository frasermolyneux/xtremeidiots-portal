namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string IdentityOid { get; set; } = string.Empty;
        public string XtremeIdiotsForumId { get; set; } = string.Empty;
    }
}
