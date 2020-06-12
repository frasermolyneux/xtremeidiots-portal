using System.Collections.Generic;
using XI.AzureTableExtensions;
using XI.AzureTableExtensions.Attributes;
using XI.Portal.Maps.Dto;

namespace XI.Portal.Maps.Models
{
    public class MapPopularityEntity : TableEntityExtended
    {
        // ReSharper disable once UnusedMember.Global - required for ExecuteQuerySegmentedAsync
        public MapPopularityEntity()
        {
        }

        public MapPopularityEntity(MapPopularityDto model)
        {
            RowKey = model.MapName;
            PartitionKey = model.GameType.ToString();
        }

        [EntityJsonPropertyConverter] public List<MapPopularityVoteDto> MapVotes { get; set; } = new List<MapPopularityVoteDto>();
    }
}