using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;

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
                MapName = map.MapName,
                MapImageUri = map.MapImageUri,
                TotalLikes = map.TotalLikes,
                TotalDislikes = map.TotalDislikes,
                TotalVotes = map.TotalVotes,
                LikePercentage = map.LikePercentage,
                DislikePercentage = map.DislikePercentage
            };

            if (!string.IsNullOrEmpty(map.MapFiles))
            {
                var mapFileDtos = JsonConvert.DeserializeObject<List<MapFileDto>>(map.MapFiles);
                if (mapFileDtos != null)
                    dto.MapFiles = mapFileDtos;
                else
                    dto.MapFiles = new List<MapFileDto>();
            }
            else
            {
                dto.MapFiles = new List<MapFileDto>();
            }

            return dto;
        }
    }
}
