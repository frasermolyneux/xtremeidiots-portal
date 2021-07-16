using System.Linq;
using XI.Portal.Maps.Dto;

namespace XI.Portal.Maps.Extensions
{
    public static class MapsExtensions
    {
        public static MapDto ToDto(this Data.Legacy.Models.Maps map, string baseUrl)
        {
            double totalLikes = map.MapVotes.Count(mv => mv.Like);
            double totalDislikes = map.MapVotes.Count(mv => !mv.Like);
            var totalVotes = map.MapVotes.Count;
            double likePercentage = 0;
            double dislikePercentage = 0;

            if (totalVotes > 0)
            {
                likePercentage = totalLikes / totalVotes * 100;
                dislikePercentage = totalDislikes / totalVotes * 100;
            }

            var mapDto = new MapDto
            {
                MapId = map.MapId,
                GameType = map.GameType,
                MapName = map.MapName,
                MapFiles = map.MapFiles.Select(mf => new MapFileDto
                {
                    FileName = mf.FileName,
                    FileUrl = $"{baseUrl}/redirect/{map.GameType.ToRedirectShortName()}/usermaps/{map.MapName}/{mf.FileName}"
                }).ToList(),
                MapVotes = map.MapVotes.Select(mv => new LegacyMapVoteDto
                {
                    Like = mv.Like
                }).ToList(),
                LikePercentage = likePercentage,
                DislikePercentage = dislikePercentage,
                TotalLikes = totalLikes,
                TotalDislikes = totalDislikes,
                TotalVotes = totalVotes
            };

            return mapDto;
        }
    }
}