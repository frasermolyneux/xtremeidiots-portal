using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Models;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessServers)]
    public class ServersController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IGameServerStatusStatsRepository _gameServerStatusStatsRepository;
        private readonly IMapsRepository _mapsRepository;
        private readonly IPlayerLocationsRepository _playerLocationsRepository;

        public ServersController(
            IGameServersRepository gameServersRepository,
            IGameServerStatusRepository gameServerStatusRepository,
            IMapsRepository mapsRepository,
            IPlayerLocationsRepository playerLocationsRepository,
            IGameServerStatusStatsRepository gameServerStatusStatsRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
            _gameServerStatusStatsRepository = gameServerStatusStatsRepository ?? throw new ArgumentNullException(nameof(gameServerStatusStatsRepository));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition,
                Filter = GameServerFilterModel.FilterBy.ShowOnPortalServerList
            };

            var servers = await _gameServersRepository.GetGameServers(filterModel);
            var serversStatus = await _gameServerStatusRepository.GetAllStatusModels(new GameServerStatusFilterModel(), TimeSpan.Zero);

            var results = new List<ServerInfoViewModel>();

            foreach (var server in servers)
                results.Add(new ServerInfoViewModel
                {
                    GameServer = server,
                    GameServerStatus = serversStatus.SingleOrDefault(ss => server.ServerId == ss.ServerId)
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
            var gameServer = await _gameServersRepository.GetGameServer(id);
            var gameServerStatusDto = await _gameServerStatusRepository.GetStatus(id,TimeSpan.Zero);

            var serversFilterModel = new GameServerStatusStatsFilterModel
            {
                ServerId = gameServer.ServerId,
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

            MapDto map = null;
            if (gameServerStatusDto != null)
                map = await _mapsRepository.GetMap(gameServerStatusDto.GameType, gameServerStatusDto.Map);

            var mapsFilterModel = new MapsFilterModel
            {
                GameType = gameServer.GameType,
                MapNames = gameServerStatusStatsDtos.GroupBy(m => m.MapName).Select(m => m.Key).ToList()
            };
            var maps = await _mapsRepository.GetMaps(mapsFilterModel);

            return View(new ServerInfoViewModel
            {
                GameServer = gameServer,
                GameServerStatus = gameServerStatusDto,
                Map = map,
                Maps = maps,
                GameServerStatusStats = gameServerStatusStatsDtos,
                MapTimelineDataPoints = mapTimelineDataPoints
            });
        }

        public class ServerInfoViewModel
        {
            public GameServerDto GameServer { get; set; }
            public PortalGameServerStatusDto GameServerStatus { get; set; }
            public MapDto Map { get; set; }
            public List<GameServerStatusStatsDto> GameServerStatusStats { get; set; }
            public List<MapTimelineDataPoint> MapTimelineDataPoints { get; set; }
            public List<MapDto> Maps { get; set; }
        }

        public class MapTimelineDataPoint
        {
            public string MapName { get; set; }
            public DateTime Start { get; set; }
            public DateTime Stop { get; set; }
        }
    }
}