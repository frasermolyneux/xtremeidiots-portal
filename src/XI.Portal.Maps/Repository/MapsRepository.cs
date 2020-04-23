using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Maps.Configuration;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Extensions;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Repository
{
    public class MapsRepository : IMapsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IMapsRepositoryOptions _options;

        public MapsRepository(IMapsRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetMapListCount(MapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new MapsFilterModel();

            return await _legacyContext.Maps.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<MapsListEntryViewModel>> GetMapList(MapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new MapsFilterModel();

            var maps = await _legacyContext.Maps.ApplyFilter(filterModel).Include(m => m.MapFiles).Include(m => m.MapVotes).ToListAsync();

            var mapsResult = new List<MapsListEntryViewModel>();

            foreach (var map in maps)
            {
                double totalLikes = map.MapVotes.Count(mv => mv.Like);
                double totalDislikes = map.MapVotes.Count(mv => !mv.Like);
                var totalVotes = map.MapVotes.Count();
                double likePercentage = 0;
                double dislikePercentage = 0;

                if (totalVotes > 0)
                {
                    likePercentage = totalLikes / totalVotes * 100;
                    dislikePercentage = totalDislikes / totalVotes * 100;
                }

                var mapListEntryViewModel = new MapsListEntryViewModel
                {
                    GameType = map.GameType.ToString(),
                    MapName = map.MapName,
                    TotalVotes = totalVotes,
                    TotalLikes = totalLikes,
                    TotalDislikes = totalDislikes,
                    LikePercentage = likePercentage,
                    DislikePercentage = dislikePercentage,
                    MapFiles = new Dictionary<string, string>()
                };

                foreach (var mapFile in map.MapFiles) mapListEntryViewModel.MapFiles.Add(mapFile.FileName, $"{_options.MapRedirectBaseUrl}/redirect/{map.GameType.ToRedirectShortName()}/usermaps/{map.MapName}/{mapFile.FileName}");

                mapsResult.Add(mapListEntryViewModel);
            }

            return mapsResult;
        }

        public async Task<MapDto> GetMap(GameType gameType, string mapName)
        {
            var map = await _legacyContext.Maps
                .Include(m => m.MapFiles)
                .Include(m => m.MapVotes)
                .SingleOrDefaultAsync(m => m.MapName == mapName && m.GameType == gameType);

            if (map == null) return null;

            var mapDto = new MapDto
            {
                MapId = map.MapId,
                GameType = map.GameType,
                MapName = map.MapName,
                MapFiles = map.MapFiles.Select(mf => new MapFileDto
                {
                    FileName = mf.FileName,
                    FileUrl = $"{_options.MapRedirectBaseUrl}/redirect/{map.GameType.ToRedirectShortName()}/usermaps/{map.MapName}/{mf.FileName}"
                }).ToList(),
                MapVotes = map.MapVotes.Select(mv => new MapVoteDto
                {
                    Like = mv.Like
                }).ToList()
            };

            return mapDto;
        }

        public async Task<List<MapRotationDto>> GetMapRotation(Guid serverId)
        {
            var mapRotations = await _legacyContext.MapRotations
                .Include(m => m.MapMap)
                .Include(m => m.MapMap.MapFiles)
                .Include(m => m.MapMap.MapVotes)
                .Where(m => m.GameServerServer.ServerId == serverId).ToListAsync();

            var results = new List<MapRotationDto>();

            foreach (var mapRotation in mapRotations)
            {
                double totalLikes = mapRotation.MapMap.MapVotes.Count(mv => mv.Like);
                double totalDislikes = mapRotation.MapMap.MapVotes.Count(mv => !mv.Like);
                var totalVotes = mapRotation.MapMap.MapVotes.Count();
                double likePercentage = 0;
                double dislikePercentage = 0;

                if (totalVotes > 0)
                {
                    likePercentage = totalLikes / totalVotes * 100;
                    dislikePercentage = totalDislikes / totalVotes * 100;
                }

                var mapDto = new MapDto
                {
                    MapId = mapRotation.MapMap.MapId,
                    GameType = mapRotation.MapMap.GameType,
                    MapName = mapRotation.MapMap.MapName,
                    MapFiles = mapRotation.MapMap.MapFiles.Select(mf => new MapFileDto
                    {
                        FileName = mf.FileName,
                        FileUrl = $"{_options.MapRedirectBaseUrl}/redirect/{mapRotation.MapMap.GameType.ToRedirectShortName()}/usermaps/{mapRotation.MapMap.MapName}/{mf.FileName}"
                    }).ToList(),
                    MapVotes = mapRotation.MapMap.MapVotes.Select(mv => new MapVoteDto
                    {
                        Like = mv.Like
                    }).ToList(),

                    LikePercentage = likePercentage,
                    DislikePercentage = dislikePercentage,
                    TotalLikes = totalLikes,
                    TotalDislikes = totalDislikes,
                    TotalVotes = totalVotes
                };

                var mapRotationDto = new MapRotationDto
                {
                    GameMode = mapRotation.GameMode,
                    Map = mapDto
                };

                results.Add(mapRotationDto);
            }

            return results;
        }
    }
}