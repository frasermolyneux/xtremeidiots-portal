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
    public class LegacyMapsRepository : ILegacyMapsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IMapVotesRepository _mapVotesRepository;
        private readonly ILegacyMapsRepositoryOptions _options;

        public LegacyMapsRepository(
            ILegacyMapsRepositoryOptions options,
            LegacyPortalContext legacyContext,
            IMapVotesRepository mapVotesRepository)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _mapVotesRepository = mapVotesRepository ?? throw new ArgumentNullException(nameof(mapVotesRepository));
        }

        public async Task<int> GetMapsCount(LegacyMapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new LegacyMapsFilterModel();

            return await _legacyContext.Maps.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<LegacyMapDto>> GetMaps(LegacyMapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new LegacyMapsFilterModel();

            var maps = await _legacyContext.Maps.ApplyFilter(filterModel).ToListAsync();

            var results = new List<LegacyMapDto>();
            foreach (var map in maps)
            {
                var mapVoteIndexCloudEntity = await _mapVotesRepository.GetMapVoteIndex(map.GameType, map.MapName);
                var mapDto = map.ToDto(mapVoteIndexCloudEntity, _options.MapRedirectBaseUrl);
                results.Add(mapDto);
            }

            return results;
        }

        public async Task<LegacyMapDto> GetMap(GameType gameType, string mapName)
        {
            var map = await _legacyContext.Maps
                .Include(m => m.MapFiles)
                .SingleOrDefaultAsync(m => m.MapName == mapName && m.GameType == gameType);

            var mapVoteIndexCloudEntity = await _mapVotesRepository.GetMapVoteIndex(gameType, mapName);

            return map?.ToDto(mapVoteIndexCloudEntity, _options.MapRedirectBaseUrl);
        }

        public async Task CreateMap(LegacyMapDto legacyMapDto)
        {
            if (legacyMapDto == null) throw new ArgumentNullException(nameof(legacyMapDto));

            var map = new Data.Legacy.Models.Maps
            {
                MapId = Guid.NewGuid(),
                GameType = legacyMapDto.GameType,
                MapName = legacyMapDto.MapName,
                MapFiles = legacyMapDto.MapFiles.Select(mf => new MapFiles
                {
                    MapFileId = Guid.NewGuid(),
                    FileName = mf.FileName
                }).ToList()
            };

            _legacyContext.Maps.Add(map);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateMap(LegacyMapDto legacyMapDto)
        {
            if (legacyMapDto == null) throw new ArgumentNullException(nameof(legacyMapDto));

            var map = await _legacyContext.Maps
                .Include(m => m.MapFiles)
                .Include(m => m.MapVotes)
                .SingleOrDefaultAsync(m => m.MapId == legacyMapDto.MapId);

            if (map == null) throw new NullReferenceException(nameof(map));

            var mapFiles = await _legacyContext.MapFiles.Where(mf => mf.MapMapId == legacyMapDto.MapId).ToListAsync();
            _legacyContext.MapFiles.RemoveRange(mapFiles);

            var newMapFiles = legacyMapDto.MapFiles.Select(mf => new MapFiles
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
                throw new Exception("Cannot delete legacyMap when it has votes");

            _legacyContext.Maps.Remove(map);
            await _legacyContext.SaveChangesAsync();
        }
    }
}