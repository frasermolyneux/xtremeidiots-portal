using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Portal.Web.Models;
using XI.Servers.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.CanAccessServerAdmin)]
    public class ServerAdminController : Controller
    {
        private readonly IChatLogsRepository _chatLogsRepository;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IRconClientFactory _rconClientFactory;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin};

        public ServerAdminController(IGameServersRepository gameServersRepository,
            IGameServerStatusRepository gameServerStatusRepository,
            IRconClientFactory rconClientFactory,
            IChatLogsRepository chatLogsRepository)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
            _chatLogsRepository = chatLogsRepository ?? throw new ArgumentNullException(nameof(chatLogsRepository));
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessLiveRcon)]
        public async Task<IActionResult> Index()
        {
            var servers = (await _gameServersRepository.GetGameServers(User, _requiredClaims))
                .Where(server => !string.IsNullOrWhiteSpace(server.RconPassword));

            return View(servers);
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessLiveRcon)]
        public async Task<IActionResult> ViewRcon(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _gameServersRepository.GetGameServer(id, User, _requiredClaims);

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessLiveRcon)]
        public async Task<IActionResult> GetRconPlayers(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _gameServerStatusRepository.GetStatus((Guid) id, User, _requiredClaims, TimeSpan.FromSeconds(15));

            return Json(new
            {
                data = model.Players
            });
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessLiveRcon)]
        public async Task<IActionResult> KickPlayer(Guid? id, string num)
        {
            if (id == null) return NotFound();

            if (string.IsNullOrWhiteSpace(num))
                return NotFound();

            var model = await _gameServersRepository.GetGameServer(id, User, _requiredClaims);

            var rconClient = _rconClientFactory.CreateInstance(model.GameType, model.ServerId, model.Hostname, model.QueryPort, model.RconPassword);

            //rconClient.KickPlayer(num);

            TempData["Success"] = $"Player in slot {num} has been kicked";
            return RedirectToAction(nameof(ViewRcon), new {id});
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessGlobalChatLog)]
        public IActionResult ChatLogIndex()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessGlobalChatLog)]
        public async Task<IActionResult> GetChatLogAjax()
        {
            return await GetChatLogPrivate(null, null);
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessGameChatLog)]
        public IActionResult GameChatLog(GameType id)
        {
            if (!User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin || claim.Value == id.ToString())) return Unauthorized();

            ViewData["GameType"] = id;
            return View(nameof(ChatLogIndex));
        }

        [HttpPost]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessGameChatLog)]
        public async Task<IActionResult> GetGameChatLogAjax(GameType id)
        {
            if (!User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin || claim.Value == id.ToString())) return Unauthorized();

            return await GetChatLogPrivate(id, null);
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessGameChatLog)]
        public IActionResult ServerChatLog(Guid id)
        {
            ViewData["ServerId"] = id;
            return View(nameof(ChatLogIndex));
        }

        [HttpPost]
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessGameChatLog)]
        public async Task<IActionResult> GetServerChatLogAjax(Guid id)
        {
            return await GetChatLogPrivate(null, id);
        }

        [HttpGet]
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
        [Authorize(Policy = XtremeIdiotsPolicy.CanAccessGlobalChatLog)]
        public async Task<IActionResult> ChatLogPermaLink(Guid? id)
        {
            if (id == null) return NotFound();

            var chatLog = await _chatLogsRepository.GetChatLog((Guid) id);
            return View(chatLog);
        }
    }
}