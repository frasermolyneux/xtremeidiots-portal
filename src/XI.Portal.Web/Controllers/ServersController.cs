using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Players.Interfaces;
using XI.Portal.Repository.Dtos;
using XI.Portal.Repository.Interfaces;
using XI.Portal.Repository.Models;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessServers)]
    public class ServersController : Controller
    {
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IGameServerStatusStatsRepository _gameServerStatusStatsRepository;
        private readonly IMapsRepository _mapsRepository;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IPlayerLocationsRepository _playerLocationsRepository;

        public ServersController(
            IGameServerStatusRepository gameServerStatusRepository,
            IPlayerLocationsRepository playerLocationsRepository,
            IGameServerStatusStatsRepository gameServerStatusStatsRepository,
            IMapsRepository mapsRepository,
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient)
        {
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
            _gameServerStatusStatsRepository = gameServerStatusStatsRepository ?? throw new ArgumentNullException(nameof(gameServerStatusStatsRepository));
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
            this.repositoryTokenProvider = repositoryTokenProvider;
            this.repositoryApiClient = repositoryApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var servers = await repositoryApiClient.GameServers.GetGameServers(accessToken, null, null, "ShowOnPortalServerList", 0, 0, "BannerServerListPosition");

            var serversStatus = await _gameServerStatusRepository.GetAllStatusModels(new GameServerStatusFilterModel(), TimeSpan.Zero);

            var results = new List<ServerInfoViewModel>();

            foreach (var server in servers)
                results.Add(new ServerInfoViewModel
                {
                    GameServer = server,
                    GameServerStatus = serversStatus.SingleOrDefault(ss => server.Id == ss.ServerId)
                });

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> Map()
        {
            var playerLocationDtos = await _playerLocationsRepository.GetLocations();

            return View(playerLocationDtos);
        }

        [HttpGet]
        public async Task<IActionResult> ServerInfo(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServer = await repositoryApiClient.GameServers.GetGameServer(accessToken, id);

            var gameServerStatusDto = await _gameServerStatusRepository.GetStatus(id, TimeSpan.Zero);

            var serversFilterModel = new GameServerStatusStatsFilterModel
            {
                ServerId = gameServer.Id,
                Cutoff = DateTime.UtcNow.AddDays(-2),
                Order = GameServerStatusStatsFilterModel.OrderBy.TimestampAsc
            };
            var gameServerStatusStatsDtos = await _gameServerStatusStatsRepository.GetGameServerStatusStats(serversFilterModel);

            var mapTimelineDataPoints = new List<MapTimelineDataPoint>();

            DateTime? timestamp = null;
            string trackMap = null;

            if (gameServerStatusStatsDtos.Any())
            {
                var lastItem = gameServerStatusStatsDtos.Last();
                foreach (var item in gameServerStatusStatsDtos.Where(gss => gss.Timestamp > DateTime.UtcNow.AddDays(-1)))
                    if (trackMap == null)
                    {
                        trackMap = item.MapName;
                        timestamp = item.Timestamp.UtcDateTime;
                    }
                    else
                    {
                        if (item == lastItem)
                        {
                            mapTimelineDataPoints.Add(new MapTimelineDataPoint
                            {
                                MapName = trackMap,
                                Start = (DateTime)timestamp,
                                Stop = item.Timestamp.UtcDateTime
                            });
                            continue;
                        }

                        if (trackMap == item.MapName) continue;

                        mapTimelineDataPoints.Add(new MapTimelineDataPoint
                        {
                            MapName = trackMap,
                            Start = (DateTime)timestamp,
                            Stop = item.Timestamp.UtcDateTime
                        });

                        timestamp = item.Timestamp.UtcDateTime;
                        trackMap = item.MapName;
                    }
            }

            LegacyMapDto mapDto = null;
            if (gameServerStatusDto != null)
                mapDto = await _mapsRepository.GetMap(gameServerStatusDto.GameType, gameServerStatusDto.Map);

            var queryOptions = new MapsQueryOptions
            {
                GameType = gameServer.GameType,
                MapNames = gameServerStatusStatsDtos.GroupBy(m => m.MapName).Select(m => m.Key).ToList()
            };
            var maps = await _mapsRepository.GetMaps(queryOptions);

            return View(new ServerInfoViewModel
            {
                GameServer = gameServer,
                GameServerStatus = gameServerStatusDto,
                Map = mapDto,
                Maps = maps,
                GameServerStatusStats = gameServerStatusStatsDtos,
                MapTimelineDataPoints = mapTimelineDataPoints
            });
        }

        public class ServerInfoViewModel
        {
            public GameServerDto GameServer { get; set; }
            public PortalGameServerStatusDto GameServerStatus { get; set; }
            public LegacyMapDto Map { get; set; }
            public List<GameServerStatusStatsDto> GameServerStatusStats { get; set; }
            public List<MapTimelineDataPoint> MapTimelineDataPoints { get; set; }
            public List<LegacyMapDto> Maps { get; set; }
        }

        public class MapTimelineDataPoint
        {
            public string MapName { get; set; }
            public DateTime Start { get; set; }
            public DateTime Stop { get; set; }
        }
    }
}