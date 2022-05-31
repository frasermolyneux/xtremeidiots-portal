using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessServerAdmin)]
    public class ServerAdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;

        public ServerAdminController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

            this.serversApiClient = serversApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.ServerAdmin };
            var (gameTypes, serverIds) = User.ClaimedGamesAndItems(requiredClaims);

            var servers = await repositoryApiClient.GameServers.GetGameServers(gameTypes, serverIds, GameServerFilter.LiveStatusEnabled, 0, 0, GameServerOrder.BannerServerListPosition);

            var results = servers.Select(gs => new ServerAdminGameServerViewModel
            {
                GameServer = gs
            }).ToList();

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> ViewRcon(Guid id)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            return View(gameServerDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetRconPlayers(Guid id)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            var serverRcoStatusResponseDto = await serversApiClient.Rcon.GetServerStatus(id);

            return Json(new
            {
                data = serverRcoStatusResponseDto.Players
            });
        }

        [HttpPost]
        public async Task<IActionResult> RestartServer(Guid id)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            //var rconClient = _rconClientFactory.CreateInstance(
            //    gameServerDto.GameType,
            //    gameServerDto.Id,
            //    gameServerDto.Hostname,
            //    gameServerDto.QueryPort,
            //    gameServerDto.RconPassword);
            //
            //var result = await rconClient.Restart();

            return Json(new
            {
                Success = true,
                //Message = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> RestartMap(Guid id)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            //var rconClient = _rconClientFactory.CreateInstance(
            //    gameServerDto.GameType,
            //    gameServerDto.Id,
            //    gameServerDto.Hostname,
            //    gameServerDto.QueryPort,
            //    gameServerDto.RconPassword);
            //
            //var result = await rconClient.RestartMap();

            return Json(new
            {
                Success = true,
                //Message = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> FastRestartMap(Guid id)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            //var rconClient = _rconClientFactory.CreateInstance(
            //    gameServerDto.GameType,
            //    gameServerDto.Id,
            //    gameServerDto.Hostname,
            //    gameServerDto.QueryPort,
            //    gameServerDto.RconPassword);
            //
            //var result = await rconClient.FastRestartMap();

            return Json(new
            {
                Success = true,
                //Message = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> NextMap(Guid id)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            //var rconClient = _rconClientFactory.CreateInstance(
            //    gameServerDto.GameType,
            //    gameServerDto.Id,
            //    gameServerDto.Hostname,
            //    gameServerDto.QueryPort,
            //    gameServerDto.RconPassword);
            //
            //var result = await rconClient.NextMap();

            return Json(new
            {
                Success = true,
                //Message = result
            });
        }

        [HttpGet]
        public async Task<IActionResult> KickPlayer(Guid id, string num)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(num))
                return NotFound();

            //var rconClient = _rconClientFactory.CreateInstance(gameServerDto.GameType, gameServerDto.Id, gameServerDto.Hostname, gameServerDto.QueryPort, gameServerDto.RconPassword);

            //rconClient.KickPlayer(num);

            TempData["Success"] = $"Player in slot {num} has been kicked";
            return RedirectToAction(nameof(ViewRcon), new { id });
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.ViewGlobalChatLog)]
        public IActionResult ChatLogIndex()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = AuthPolicies.ViewGlobalChatLog)]
        public async Task<IActionResult> GetChatLogAjax()
        {
            return await GetChatLogPrivate(null, null, null);
        }

        [HttpGet]
        public async Task<IActionResult> GameChatLog(GameType id)
        {
            var canViewGameChatLog = await _authorizationService.AuthorizeAsync(User, id, AuthPolicies.ViewGameChatLog);

            if (!canViewGameChatLog.Succeeded)
                return Unauthorized();

            ViewData["GameType"] = id;
            return View(nameof(ChatLogIndex));
        }

        [HttpPost]
        public async Task<IActionResult> GetGameChatLogAjax(GameType id)
        {
            var canViewGameChatLog = await _authorizationService.AuthorizeAsync(User, id, AuthPolicies.ViewGameChatLog);

            if (!canViewGameChatLog.Succeeded)
                return Unauthorized();

            return await GetChatLogPrivate(id, null, null);
        }

        [HttpGet]
        public async Task<IActionResult> ServerChatLog(Guid id)
        {

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerDto == null)
                return NotFound();

            var canViewServerChatLog = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewServerChatLog);

            if (!canViewServerChatLog.Succeeded)
                return Unauthorized();

            ViewData["ServerId"] = id;
            return View(nameof(ChatLogIndex));
        }

        [HttpPost]
        public async Task<IActionResult> GetServerChatLogAjax(Guid id)
        {

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerDto == null)
                return NotFound();

            var canViewServerChatLog = await _authorizationService.AuthorizeAsync(User, gameServerDto.GameType, AuthPolicies.ViewServerChatLog);

            if (!canViewServerChatLog.Succeeded)
                return Unauthorized();

            return await GetChatLogPrivate(null, id, null);
        }

        [HttpPost]
        public async Task<IActionResult> GetPlayerChatLog(Guid id)
        {

            var playerDto = await repositoryApiClient.Players.GetPlayer(id);

            if (playerDto == null)
                return NotFound();

            var canViewGameChatLog = await _authorizationService.AuthorizeAsync(User, playerDto.GameType, AuthPolicies.ViewGameChatLog);

            if (!canViewGameChatLog.Succeeded)
                return Unauthorized();

            return await GetChatLogPrivate(playerDto.GameType, null, playerDto.Id);
        }

        private async Task<IActionResult> GetChatLogPrivate(GameType? gameType, Guid? serverId, Guid? playerId)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var order = "TimestampDesc";
            if (model.Order != null)
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "timestamp":
                        order = searchOrder == "asc" ? "TimestampAsc" : "TimestampDesc";
                        break;
                }
            }


            var searchResponse = await repositoryApiClient.ChatMessages.SearchChatMessages(gameType, serverId, playerId, model.Search?.Value, model.Length, model.Start, order);

            return Json(new
            {
                model.Draw,
                recordsTotal = searchResponse.TotalRecords,
                recordsFiltered = searchResponse.FilteredRecords,
                data = searchResponse.Entries
            });
        }

        [HttpGet]
        public async Task<IActionResult> ChatLogPermaLink(Guid id)
        {

            var chatMessage = await repositoryApiClient.ChatMessages.GetChatMessage(id);

            if (chatMessage == null)
                return NotFound();

            return View(chatMessage);
        }
    }
}