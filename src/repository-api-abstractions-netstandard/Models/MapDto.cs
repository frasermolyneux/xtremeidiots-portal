using System;
using System.Collections.Generic;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Repository.Dtos
{
    public class MapDto
    {
        public Guid MapId { get; set; }
        public GameType GameType { get; set; }
        public string MapName { get; set; } = string.Empty;

        public List<MapFileDto> MapFiles { get; set; } = new List<MapFileDto>();
    }
}