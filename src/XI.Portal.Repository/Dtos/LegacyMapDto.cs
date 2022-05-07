using System.Collections.Generic;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Repository.Dtos
{
    public class LegacyMapDto
    {
        public GameType GameType { get; set; }
        public string MapName { get; set; }
        public int PositiveVotes { get; set; }
        public int NegativeVotes { get; set; }
        public int TotalVotes { get; set; }
        public double PositivePercentage { get; set; }
        public double NegativePercentage { get; set; }
        public List<LegacyMapFileDto> MapFiles { get; set; }
    }
}