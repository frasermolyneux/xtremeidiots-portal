using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class MapDto
    {
        public Guid MapId { get; set; }
        public GameType GameType { get; set; }
        public string MapName { get; set; } = string.Empty;

        public List<MapFileDto> MapFiles { get; set; } = new List<MapFileDto>();
    }
}