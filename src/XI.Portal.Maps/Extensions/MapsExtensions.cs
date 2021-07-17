using System.Linq;
using XI.Portal.Maps.Dto;
using XI.Portal.Repository.CloudEntities;

namespace XI.Portal.Maps.Extensions
{
    public static class MapsExtensions
    {
        public static LegacyMapDto ToDto(this Data.Legacy.Models.Maps map, MapVoteIndexCloudEntity mapVoteIndexCloudEntity, string baseUrl)
        {
            var mapDto = new LegacyMapDto
            {
                MapId = map.MapId,
                GameType = map.GameType,
                MapName = map.MapName,
                MapFiles = map.MapFiles.Select(mf => new MapFileDto
                {
                    FileName = mf.FileName,
                    FileUrl = $"{baseUrl}/redirect/{map.GameType.ToRedirectShortName()}/usermaps/{map.MapName}/{mf.FileName}"
                }).ToList()
            };

            if (mapVoteIndexCloudEntity != null)
            {
                double likePercentage = 0;
                double dislikePercentage = 0;

                if (mapVoteIndexCloudEntity.TotalVotes > 0)
                {
                    likePercentage = (double)mapVoteIndexCloudEntity.PositiveVotes / mapVoteIndexCloudEntity.TotalVotes * 100;
                    dislikePercentage = (double)mapVoteIndexCloudEntity.NegativeVotes / mapVoteIndexCloudEntity.TotalVotes * 100;
                }

                mapDto.LikePercentage = likePercentage;
                mapDto.DislikePercentage = dislikePercentage;
                mapDto.TotalLikes = mapVoteIndexCloudEntity.PositiveVotes;
                mapDto.TotalDislikes = mapVoteIndexCloudEntity.NegativeVotes;
                mapDto.TotalVotes = mapVoteIndexCloudEntity.TotalVotes;
            }

            return mapDto;
        }
    }
}