using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Repository.Dtos
{
    public class MapVoteDto
    {
        public GameType GameType { get; set; }
        public string MapName { get; set; }
        public string Guid { get; set; }
        public bool Like { get; set; }
    }
}