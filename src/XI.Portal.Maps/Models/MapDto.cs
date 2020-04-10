using XI.CommonTypes;

namespace XI.Portal.Maps.Models
{
    public class MapDto : IMapDto
    {
        public string RowKey { get; set; }
        public GameType GameType { get; set; }
    }
}