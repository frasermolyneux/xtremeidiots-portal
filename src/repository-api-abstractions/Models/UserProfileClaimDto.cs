namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class UserProfileClaimDto
    {
        public Guid Id { get; set; }
        public Guid UserProfileId { get; set; }
        public bool SystemGenerated { get; set; }
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
    }
}
