using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class MapDto
    {
        public Guid MapId { get; set; }
        public GameType GameType { get; set; }
        public string MapName { get; set; } = string.Empty;
        public string MapImageUri { get; set; } = string.Empty;

        public int TotalLikes { get; set; } = 0;
        public int TotalDislikes { get; set; } = 0;
        public int TotalVotes { get; set; } = 0;
        public double LikePercentage { get; set; } = 0;
        public double DislikePercentage { get; set; } = 0;

        public List<MapFileDto> MapFiles { get; set; } = new List<MapFileDto>();
    }
}