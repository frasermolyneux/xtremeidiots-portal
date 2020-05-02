using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.ServerAdmin.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Portal.Web.Models;
using XI.Servers.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessServerAdmin)]
    public class ServerAdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IChatLogsRepository _chatLogsRepository;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IRconClientFactory _rconClientFactory;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin};

        public ServerAdminController(
            IAuthorizationService authorizationService,
            IGameServersRepository gameServersRepository,
            IGameServerStatusRepository gameServerStatusRepository,
            IRconClientFactory rconClientFactory,
            IChatLogsRepository chatLogsRepository)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
            _chatLogsRepository = chatLogsRepository ?? throw new ArgumentNullException(nameof(chatLogsRepository));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuthForServerAdmin(User);

            var servers = await _gameServersRepository.GetGameServers(filterModel);

            var serversStatus = await _gameServerStatusRepository.GetAllStatusModels(User, _requiredClaims, TimeSpan.Zero);

            var results = new List<ServersController.ServerInfoViewModel>();

            foreach (var server in servers)
            {
                var portalGameServerStatusDto = serversStatus.SingleOrDefault(ss => server.ServerId == ss.ServerId);

                if (portalGameServerStatusDto != null)
                    results.Add(new ServersController.ServerInfoViewModel
                    {
                        GameServer = server,
                        GameServerStatus = portalGameServerStatusDto
                    });
            }

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> ViewRcon(Guid id)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            return View(gameServerDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetRconPlayers(Guid id)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            var portalGameServerStatusDto = await _gameServerStatusRepository.GetStatus(id, User, _requiredClaims, TimeSpan.FromSeconds(15));

            return Json(new
            {
                data = portalGameServerStatusDto.Players
            });
        }

        [HttpGet]
        public async Task<IActionResult> KickPlayer(Guid id, string num)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(id);

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(num))
                return NotFound();

            var model = await _gameServersRepository.GetGameServer(id);

            var rconClient = _rconClientFactory.CreateInstance(model.GameType, model.ServerId, model.Hostname, model.QueryPort, model.RconPassword);

            //rconClient.KickPlayer(num);

            TempData["Success"] = $"Player in slot {num} has been kicked";
            return RedirectToAction(nameof(ViewRcon), new {id});
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
            return await GetChatLogPrivate(null, null);
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

            return await GetChatLogPrivate(id, null);
        }

        [HttpGet]
        public async Task<IActionResult> ServerChatLog(Guid id)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(id);

            if (gameServerDto == null)
                return NotFound();

            var canViewServerChatLog = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.ViewServerChatLog);

            if (!canViewServerChatLog.Succeeded)
                return Unauthorized();

            ViewData["ServerId"] = id;
            return View(nameof(ChatLogIndex));
        }

        [HttpPost]
        public async Task<IActionResult> GetServerChatLogAjax(Guid id)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(id);

            if (gameServerDto == null)
                return NotFound();

            var canViewServerChatLog = await _authorizationService.AuthorizeAsync(User, gameServerDto, AuthPolicies.ViewServerChatLog);

            if (!canViewServerChatLog.Succeeded)
                return Unauthorized();

            return await GetChatLogPrivate(null, id);
        }

        private async Task<IActionResult> GetChatLogPrivate(GameType? gameType, Guid? serverId)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var filterModel = new ChatLogFilterModel();

            if (gameType != null)
                filterModel.GameType = (GameType) gameType;

            if (serverId != null)
                filterModel.ServerId = (Guid) serverId;

            var recordsTotal = await _chatLogsRepository.GetChatLogCount(filterModel);

            filterModel.FilterString = model.Search?.Value;
            var recordsFiltered = await _chatLogsRepository.GetChatLogCount(filterModel);

            filterModel.TakeEntries = model.Length;
            filterModel.SkipEntries = model.Start;

            if (model.Order == null)
            {
                filterModel.Order = ChatLogFilterModel.OrderBy.TimestampDesc;
            }
            else
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "timestamp":
                        filterModel.Order = searchOrder == "asc" ? ChatLogFilterModel.OrderBy.TimestampAsc : ChatLogFilterModel.OrderBy.TimestampDesc;
                        break;
                }
            }

            var mapListEntries = await _chatLogsRepository.GetChatLogs(filterModel);

            return Json(new
            {
                model.Draw,
                recordsTotal,
                recordsFiltered,
                data = mapListEntries
            });
        }

        [HttpGet]
        public async Task<IActionResult> ChatLogPermaLink(Guid id)
        {
            var chatLog = await _chatLogsRepository.GetChatLog(id);

            if (chatLog == null)
                return NotFound();

            return View(chatLog);
        }
    }
}