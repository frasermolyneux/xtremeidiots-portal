namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class GameServerDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string GameType { get; set; }
        public string IpAddress { get; set; }
        public int QueryPort { get; set; }
        public bool HasFtp { get; set; }
    }
}