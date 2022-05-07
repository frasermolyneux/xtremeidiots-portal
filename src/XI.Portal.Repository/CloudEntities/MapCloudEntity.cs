using FM.AzureTableExtensions.Library;
using FM.AzureTableExtensions.Library.Attributes;
using System.Collections.Generic;
using XI.Portal.Repository.Dtos;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Repository.CloudEntities
{
    public class MapCloudEntity : TableEntityExtended
    {
        public MapCloudEntity()
        {
        }

        public MapCloudEntity(GameType gameType, string mapName)
        {
            PartitionKey = gameType.ToString();
            RowKey = mapName;
        }

        public MapCloudEntity(LegacyMapDto mapDto)
        {
            PartitionKey = mapDto.GameType.ToString();
            RowKey = mapDto.MapName;

            TotalVotes = mapDto.TotalVotes;
            PositiveVotes = mapDto.PositiveVotes;
            NegativeVotes = mapDto.NegativeVotes;
            PositivePercentage = mapDto.PositivePercentage;
            NegativePercentage = mapDto.NegativePercentage;
            MapFiles = mapDto.MapFiles;
        }

        public int TotalVotes { get; set; }
        public int PositiveVotes { get; set; }
        public int NegativeVotes { get; set; }
        public double PositivePercentage { get; set; }
        public double NegativePercentage { get; set; }

        [EntityJsonPropertyConverter]
        public List<LegacyMapFileDto> MapFiles { get; set; }
    }
}