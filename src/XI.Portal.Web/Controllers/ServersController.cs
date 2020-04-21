using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [AllowAnonymous]
    public class ServersController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IMapsRepository _mapsRepository;
        private readonly IPlayerLocationsRepository _playerLocationsRepository;

        public ServersController(IGameServersRepository gameServersRepository,
            IGameServerStatusRepository gameServerStatusRepository,
            IMapsRepository mapsRepository,
            IPlayerLocationsRepository playerLocationsRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var servers = (await _gameServersRepository.GetGameServers(null, null)).Where(server => server.ShowOnPortalServerList);
            var serversStatus = await _gameServerStatusRepository.GetAllStatusModels(null, null, TimeSpan.FromMinutes(-15));

            var locations = await _playerLocationsRepository.GetLocations();

            var results = new List<ServerInfoViewModel>();

            foreach (var server in servers)
                results.Add(new ServerInfoViewModel
                {
                    GameServer = server,
                    GameServerStatus = serversStatus.SingleOrDefault(ss => server.ServerId == ss.ServerId)
                });

            return View(new ServerIndexViewModel
            {
                ServerInfoViewModels = results,
                Locations = locations
            });
        }

        [HttpGet]
        public async Task<IActionResult> ServerInfo(Guid? id)
        {
            if (id == null)
                return NotFound();

            var gameServer = await _gameServersRepository.GetGameServer(id, null, null);
            var gameServerStatusDto = await _gameServerStatusRepository.GetStatus((Guid) id, null, null, TimeSpan.FromMinutes(-15));

            MapDto map = null;
            if (gameServerStatusDto != null)
                map = await _mapsRepository.GetMap(gameServerStatusDto.GameType, gameServerStatusDto.Map);

            var mapRotation = await _mapsRepository.GetMapRotation((Guid) id);

            return View(new ServerInfoViewModel
            {
                GameServer = gameServer,
                GameServerStatus = gameServerStatusDto,
                Map = map,
                MapRotation = mapRotation
            });
        }

        public class ServerIndexViewModel
        {
            public List<ServerInfoViewModel> ServerInfoViewModels { get; set; }
            public List<PlayerLocationDto> Locations { get; set; }
        }

        public class ServerInfoViewModel
        {
            public GameServers GameServer { get; set; }
            public PortalGameServerStatusDto GameServerStatus { get; set; }
            public MapDto Map { get; set; }
            public List<MapRotationDto> MapRotation { get; set; }
        }
    }
}