using Microsoft.Azure.Cosmos.Table;
using XI.CommonTypes;

namespace XI.Portal.Maps.Models
{
    internal class MapEntity : TableEntity, IMapDto
    {
        public GameType GameType { get; set; }
    }
}