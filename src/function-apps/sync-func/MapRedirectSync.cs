using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.SyncFunc.Redirect;

namespace XtremeIdiots.Portal.SyncFunc
{
    public class MapRedirectSync
    {
        private readonly string[] _defaultMaps =
        {
            "mp_ambush", "mp_backlot", "mp_bloc", "mp_bog", "mp_broadcast", "mp_chinatown", "mp_countdown", "mp_crash", "mp_creek", "mp_crossfire",
            "mp_district", "mp_downpour", "mp_killhouse", "mp_overgrown", "mp_pipeline", "mp_shipment", "mp_showdown", "mp_strike", "mp_vacant", "mp_cargoship",
            "mp_airfield", "mp_asylum", "mp_castle", "mp_cliffside", "mp_courtyard", "mp_dome", "mp_downfall", "mp_hanger", "mp_makin", "mp_outskirts", "mp_roundhouse",
            "mp_seelow", "mp_upheaval"
        };

        public MapRedirectSync(
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient,
            IMapRedirectRepository mapRedirectRepository)
        {
            RepositoryTokenProvider = repositoryTokenProvider;
            RepositoryApiClient = repositoryApiClient;
            MapRedirectRepository = mapRedirectRepository;
        }

        public IRepositoryTokenProvider RepositoryTokenProvider { get; }
        public IRepositoryApiClient RepositoryApiClient { get; }
        public IMapRedirectRepository MapRedirectRepository { get; }

        [FunctionName("MapRedirectSync")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunMapRedirectSync([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunMapRedirectSync @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var gamesToSync = new Dictionary<GameType, string>
            {
                {GameType.CallOfDuty4, "cod4"},
                {GameType.CallOfDuty5, "cod5"}
            };

            var accessToken = await RepositoryTokenProvider.GetRepositoryAccessToken();

            foreach (var game in gamesToSync)
            {
                var mapRedirectEntries = MapRedirectRepository.GetMapEntriesForGame(game.Value);

                //var mapDtos = await RepositoryApiClient.Maps.GetMaps(accessToken, game.Key, null, null, null, null, null);





                //var queryOptions = new MapsQueryOptions
                //{
                //    GameType = game.Key
                //};
                //var mapDatabaseEntries = await _mapsRepository.GetMaps(queryOptions);

                //log.LogDebug("Total maps retrieved from redirect for {game} is {redirectMapCount} and database is {databaseMapCount}", game, mapRedirectEntries.Count, mapDatabaseEntries.Count);

                //foreach (var mapDto in mapDatabaseEntries)
                //    if (mapRedirectEntries.Any(mre => mre.MapName == mapDto.MapName))
                //    {
                //        var mapRedirectEntry = mapRedirectEntries.Single(mre => mre.MapName == mapDto.MapName);
                //        var mapFiles = mapRedirectEntry.MapFiles.Where(file => file.EndsWith(".iwd") | file.EndsWith(".ff")).ToList();

                //        if (mapFiles.Count != mapDto.MapFiles.Count)
                //        {
                //            log.LogDebug("Map {MapName} map file count differs", mapDto.MapName);

                //            mapDto.MapFiles = mapFiles.Select(mf => new MapFileDto
                //            {
                //                FileName = mf,
                //                Url = $"https://redirect.xtremeidiots.net/redirect/{mapDto.GameType.ToRedirectShortName()}/usermaps/{mapDto.MapName}/{mf}"
                //            }).ToList();

                //            await _mapsRepository.InsertOrMergeMap(mapDto);
                //        }
                //    }
                //    else
                //    {
                //        if (!_defaultMaps.Contains(mapDto.MapName))
                //            try
                //            {
                //                if (mapDto.TotalVotes > 0)
                //                {
                //                    mapDto.MapFiles = new List<MapFileDto>();
                //                    await _mapsRepository.InsertOrMergeMap(mapDto);
                //                }
                //            }
                //            catch (Exception ex)
                //            {
                //                log.LogError(ex, "Error deleting map from database");
                //            }
                //    }

                //foreach (var mapRedirectEntry in mapRedirectEntries)
                //    if (mapDatabaseEntries.All(mde => mde.MapName != mapRedirectEntry.MapName))
                //    {
                //        log.LogDebug("Map {MapName} is missing from the database", mapRedirectEntry.MapName);

                //        var mapFiles = mapRedirectEntry.MapFiles.Where(file => file.EndsWith(".iwd") | file.EndsWith(".ff")).ToList();

                //        var mapDto = new MapDto
                //        {
                //            GameType = game.Key,
                //            MapName = mapRedirectEntry.MapName,
                //            MapFiles = mapFiles.Select(mf => new MapFileDto
                //            {
                //                FileName = mf,
                //                Url = $"https://redirect.xtremeidiots.net/redirect/{game.Key.ToRedirectShortName()}/usermaps/{mapRedirectEntry.MapName}/{mf}"
                //            }).ToList()
                //        };

                //        await _mapsRepository.InsertOrMergeMap(mapDto);
                //    }
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunMapRedirectSync @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}