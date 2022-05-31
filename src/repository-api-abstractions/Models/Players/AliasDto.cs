namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class AliasDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Added { get; set; }
        public DateTime LastUsed { get; set; }
    }
}