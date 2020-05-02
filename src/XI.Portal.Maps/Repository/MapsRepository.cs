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

        public async Task<List<MapDto>> GetMapList(MapsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new MapsFilterModel();

            var maps = await _legacyContext.Maps.ApplyFilter(filterModel).ToListAsync();

            var results = maps.Select(m => m.ToDto(_options.MapRedirectBaseUrl)).ToList();

            return results;
        }

        public async Task<MapDto> GetMap(GameType gameType, string mapName)
        {
            var map = await _legacyContext.Maps
                .Include(m => m.MapFiles)
                .Include(m => m.MapVotes)
                .SingleOrDefaultAsync(m => m.MapName == mapName && m.GameType == gameType);

            return map?.ToDto(_options.MapRedirectBaseUrl);
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
                var mapDto = mapRotation.MapMap.ToDto(_options.MapRedirectBaseUrl);

                var mapRotationDto = new MapRotationDto
                {
                    GameMode = mapRotation.GameMode,
                    Map = mapDto
                };

                results.Add(mapRotationDto);
            }

            return results;
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