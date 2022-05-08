using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class MapExtensions
    {
        public static MapDto ToDto(this Map map)
        {
            var dto = new MapDto
            {
                MapId = map.MapId,
                GameType = map.GameType.ToGameType(),
                MapName = map.MapName
            };

            if (string.IsNullOrEmpty(map.MapFiles))
            {
                var mapFileDtos = JsonConvert.DeserializeObject<List<MapFileDto>>(map.MapFiles);
                if (mapFileDtos != null)
                    dto.MapFiles = mapFileDtos;
                else
                    dto.MapFiles = new List<MapFileDto>();
            }

            return dto;
        }
    }
}
