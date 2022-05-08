using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
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

            //As terrible as this is I need to see the token returned as something is wrong
            log.LogInformation($"TOKEN :: {accessToken}");

            foreach (var game in gamesToSync)
            {
                var mapRedirectEntries = MapRedirectRepository.GetMapEntriesForGame(game.Value);
                var mapsResponseDto = await RepositoryApiClient.Maps.GetMaps(accessToken, game.Key, null, null, null, null, null);

                log.LogInformation($"Total maps retrieved from redirect for {game} is {mapRedirectEntries.Count} and database is {mapsResponseDto.Entries.Count}");

                var mapDtosToCreate = new List<MapDto>();
                var mapDtosToUpdate = new List<MapDto>();

                foreach (var mapRedirectEntry in mapRedirectEntries)
                {
                    var mapDto = mapsResponseDto.Entries.SingleOrDefault(m => m.GameType == game.Key && m.MapName == mapRedirectEntry.MapName);

                    if (mapDto == null)
                    {
                        mapDtosToCreate.Add(new MapDto
                        {
                            MapName = mapRedirectEntry.MapName,
                            GameType = game.Key,
                            MapFiles = mapRedirectEntry.MapFiles.Select(mf => new MapFileDto
                            {
                                FileName = mf,
                                Url = $"https://redirect.xtremeidiots.net/redirect/{game.Value}/usermaps/{mapDto.MapName}/{mf}"
                            }).ToList(),
                        });
                    }
                    else
                    {
                        var mapFileCount = mapRedirectEntry.MapFiles.Where(file => file.EndsWith(".iwd") | file.EndsWith(".ff")).ToList();
                        if (mapFileCount.Count != mapDto.MapFiles.Count)
                        {
                            mapDto.MapFiles = mapRedirectEntry.MapFiles.Select(mf => new MapFileDto
                            {
                                FileName = mf,
                                Url = $"https://redirect.xtremeidiots.net/redirect/{game.Value}/usermaps/{mapDto.MapName}/{mf}"
                            }).ToList();

                            mapDtosToUpdate.Add(mapDto);
                        }

                    }
                }

                log.LogInformation($"Creating {mapDtosToCreate.Count} new maps and updating {mapDtosToUpdate.Count} existing maps");

                await RepositoryApiClient.Maps.CreateMaps(accessToken, mapDtosToCreate);
                await RepositoryApiClient.Maps.UpdateMaps(accessToken, mapDtosToUpdate);
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunMapRedirectSync @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}