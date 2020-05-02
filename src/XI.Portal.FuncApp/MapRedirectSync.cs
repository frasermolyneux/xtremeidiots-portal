using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Models;

namespace XI.Portal.FuncApp
{
    public class MapRedirectSync
    {
        private readonly IMapRedirectRepository _mapRedirectRepository;
        private readonly IMapsRepository _mapsRepository;

        private readonly string[] defaultMaps =
        {
            "mp_ambush", "mp_backlot", "mp_bloc", "mp_bog", "mp_broadcast", "mp_chinatown", "mp_countdown", "mp_crash", "mp_creek", "mp_crossfire",
            "mp_district", "mp_downpour", "mp_killhouse", "mp_overgrown", "mp_pipeline", "mp_shipment", "mp_showdown", "mp_strike", "mp_vacant", "mp_cargoship",
            "mp_airfield", "mp_asylum", "mp_castle", "mp_cliffside", "mp_courtyard", "mp_dome", "mp_downfall", "mp_hanger", "mp_makin", "mp_outskirts", "mp_roundhouse",
            "mp_seelow", "mp_upheaval"
        };

        public MapRedirectSync(IMapsRepository mapsRepository, IMapRedirectRepository mapRedirectRepository)
        {
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
            _mapRedirectRepository = mapRedirectRepository ?? throw new ArgumentNullException(nameof(mapRedirectRepository));
        }

        [FunctionName("MapRedirectSync")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Starting map redirect sync: {DateTime.Now}");

            var gamesToSync = new Dictionary<GameType, string>
            {
                {GameType.CallOfDuty4, "cod4"},
                {GameType.CallOfDuty5, "cod5"}
            };

            foreach (var game in gamesToSync)
            {
                var mapRedirectEntries = _mapRedirectRepository.GetMapEntriesForGame(game.Value);
                var mapsFilterModel = new MapsFilterModel
                {
                    GameType = game.Key
                };
                var mapDatabaseEntries = await _mapsRepository.GetMapList(mapsFilterModel);

                log.LogInformation("Total maps retrieved from redirect for {game} is {redirectMapCount} and database is {databaseMapCount}", game, mapRedirectEntries.Count, mapDatabaseEntries.Count);

                foreach (var mapDto in mapDatabaseEntries)
                    if (mapRedirectEntries.Any(mre => mre.MapName == mapDto.MapName))
                    {
                        var mapRedirectEntry = mapRedirectEntries.Single(mre => mre.MapName == mapDto.MapName);
                        var mapFiles = mapRedirectEntry.MapFiles.Where(file => file.EndsWith(".iwd") | file.EndsWith(".ff")).ToList();

                        if (mapFiles.Count != mapDto.MapFiles.Count)
                        {
                            log.LogInformation("Map {MapName} map file count differs", mapDto.MapName);

                            mapDto.MapFiles = mapFiles.Select(mf => new MapFileDto
                            {
                                FileName = mf
                            }).ToList();

                            await _mapsRepository.UpdateMap(mapDto);
                        }
                    }
                    else
                    {
                        if (!defaultMaps.Contains(mapDto.MapName))
                            try
                            {
                                if (mapDto.TotalVotes > 0)
                                {
                                    mapDto.MapFiles = new List<MapFileDto>();
                                    await _mapsRepository.UpdateMap(mapDto);
                                }
                                else
                                {
                                    log.LogInformation("Deleting {MapName} as it is not on the redirect", mapDto.MapName);
                                    await _mapsRepository.DeleteMap(mapDto.MapId);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.LogError(ex, "Error deleting map from database");
                            }
                    }

                foreach (var mapRedirectEntry in mapRedirectEntries)
                    if (mapDatabaseEntries.All(mde => mde.MapName != mapRedirectEntry.MapName))
                    {
                        log.LogInformation("Map {MapName} is missing from the database", mapRedirectEntry.MapName);

                        var mapFiles = mapRedirectEntry.MapFiles.Where(file => file.EndsWith(".iwd") | file.EndsWith(".ff")).ToList();

                        var mapDto = new MapDto
                        {
                            GameType = game.Key,
                            MapName = mapRedirectEntry.MapName,
                            MapFiles = mapFiles.Select(mf => new MapFileDto
                            {
                                FileName = mf
                            }).ToList()
                        };

                        await _mapsRepository.CreateMap(mapDto);
                    }
            }

            log.LogInformation("Map Redirect Sync Completed");
        }
    }
}