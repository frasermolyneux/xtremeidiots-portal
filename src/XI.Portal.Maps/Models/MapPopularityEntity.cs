using System.Collections.Generic;
using FM.AzureTableExtensions.Library;
using FM.AzureTableExtensions.Library.Attributes;
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
            MapVotes = model.MapVotes;
        }

        [EntityJsonPropertyConverter] public List<MapPopularityVoteDto> MapVotes { get; set; } = new List<MapPopularityVoteDto>();
    }
}