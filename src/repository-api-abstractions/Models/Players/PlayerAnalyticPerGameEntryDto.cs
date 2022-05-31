namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayerAnalyticPerGameEntryDto
    {
        public DateTime Created { get; set; }
        public Dictionary<string, int> GameCounts { get; set; } = new Dictionary<string, int>();
    }
}