using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Extensions;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Models;
using XI.Portal.Repository.Interfaces;

namespace XI.Portal.Maps.Repository
{
    public class MapsRepository : IMapsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IMapVotesRepository _mapVotesRepository;
        private readonly IMapsRepositoryOptions _options;

        public MapsRepository(
            IMapsRepositoryOptions options,
            LegacyPortalContext legacyContext,
            IMapVotesRepository mapVotesRepository)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _mapVotesRepository = mapVotesRepository ?? throw new ArgumentNullException(nameof(mapVotesRepository));
        }

        public async Task<int> GetMapsCount(MapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new MapsFilterModel();

            return await _legacyContext.Maps.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<MapDto>> GetMaps(MapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new MapsFilterModel();

            var maps = await _legacyContext.Maps.ApplyFilter(filterModel).ToListAsync();

            var results = new List<MapDto>();
            foreach (var map in maps)
            {
                var mapVoteIndexCloudEntity = await _mapVotesRepository.GetMapVoteIndex(map.GameType, map.MapName);
                var mapDto = map.ToDto(mapVoteIndexCloudEntity, _options.MapRedirectBaseUrl);
                results.Add(mapDto);
            }

            return results;
        }

        public async Task<MapDto> GetMap(GameType gameType, string mapName)
        {
            var map = await _legacyContext.Maps
                .Include(m => m.MapFiles)
                .SingleOrDefaultAsync(m => m.MapName == mapName && m.GameType == gameType);

            var mapVoteIndexCloudEntity = await _mapVotesRepository.GetMapVoteIndex(gameType, mapName);

            return map?.ToDto(mapVoteIndexCloudEntity, _options.MapRedirectBaseUrl);
        }

        public async Task CreateMap(MapDto mapDto)
        {
            if (mapDto == null) throw new ArgumentNullException(nameof(mapDto));

            var map = new Data.Legacy.Models.Maps
            {
                MapId = Guid.NewGuid(),
                GameType = mapDto.GameType,
                MapName = mapDto.MapName,
                MapFiles = mapDto.MapFiles.Select(mf => new MapFiles
                {
                    MapFileId = Guid.NewGuid(),
                    FileName = mf.FileName
                }).ToList()
            };

            _legacyContext.Maps.Add(map);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateMap(MapDto mapDto)
        {
            if (mapDto == null) throw new ArgumentNullException(nameof(mapDto));

            var map = await _legacyContext.Maps
                .Include(m => m.MapFiles)
                .Include(m => m.MapVotes)
                .SingleOrDefaultAsync(m => m.MapId == mapDto.MapId);

            if (map == null) throw new NullReferenceException(nameof(map));

            var mapFiles = await _legacyContext.MapFiles.Where(mf => mf.MapMapId == mapDto.MapId).ToListAsync();
            _legacyContext.MapFiles.RemoveRange(mapFiles);

            var newMapFiles = mapDto.MapFiles.Select(mf => new MapFiles
            {
                MapFileId = Guid.NewGuid(),
                FileName = mf.FileName,
                MapMap = map
            }).ToList();
            _legacyContext.MapFiles.AddRange(newMapFiles);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task DeleteMap(Guid mapId)
        {
            var map = await _legacyContext.Maps
                .Include(m => m.MapFiles)
                .Include(m => m.MapVotes)
                .Include(m => m.MapRotations)
                .SingleOrDefaultAsync(m => m.MapId == mapId);

            if (map == null)
                throw new NullReferenceException(nameof(map));

            if (map.MapVotes.Count > 0)
                throw new Exception("Cannot delete map when it has votes");

            _legacyContext.Maps.Remove(map);
            await _legacyContext.SaveChangesAsync();
        }
    }
}