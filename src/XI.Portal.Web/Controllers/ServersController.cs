using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Repository;
using XI.Portal.Servers.Interfaces;
using XI.Servers.Dto;

namespace XI.Portal.Web.Controllers
{
    [AllowAnonymous]
    public class ServersController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IMapsRepository _mapsRepository;

        public ServersController(IGameServersRepository gameServersRepository, IGameServerStatusRepository gameServerStatusRepository, IMapsRepository mapsRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _mapsRepository = mapsRepository ?? throw new ArgumentNullException(nameof(mapsRepository));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var servers = await _gameServerStatusRepository.GetAllStatusModels(null, null, TimeSpan.FromMinutes(-15));
            return View(servers);
        }

        [HttpGet]
        public async Task<IActionResult> ServerInfo(Guid? id)
        {
            if (id == null)
                return NotFound();

            var gameServer = await _gameServersRepository.GetGameServer(id, null, null);
            var gameServerStatusDto = await _gameServerStatusRepository.GetStatus((Guid) id, null, null, TimeSpan.FromMinutes(-15));

            var map = await _mapsRepository.GetMap(gameServerStatusDto.GameType, gameServerStatusDto.Map);
            var mapRotation = await _mapsRepository.GetMapRotation((Guid) id);

            return View(new ServerInfoViewModel
            {
                GameServer = gameServer,
                GameServerStatus = gameServerStatusDto,
                Map = map,
                MapRotation = mapRotation
            });
        }

        public class ServerInfoViewModel
        {
            public GameServers GameServer { get; set; }
            public GameServerStatusDto GameServerStatus { get; set; }
            public MapDto Map { get; set; }
            public List<MapRotationDto> MapRotation { get; set; }
        }
    }
}