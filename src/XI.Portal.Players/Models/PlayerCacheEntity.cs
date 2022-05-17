using FM.AzureTableExtensions.Library;
using FM.AzureTableExtensions.Library.Attributes;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Players.Models
{
    public class PlayerCacheEntity : TableEntityExtended
    {
        public Guid PlayerId { get; set; }
        [EntityEnumPropertyConverter] public GameType GameType { get; set; }
        public string Username { get; set; }
        public string Guid { get; set; }
        public string IpAddress { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
    }
}