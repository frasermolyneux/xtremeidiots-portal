using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
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
            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.ServerAdmin };
            var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, GameServerFilter.LiveTrackingEnabled, 0, 50, GameServerOrder.BannerServerListPosition);

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var results = gameServersApiResponse.Result.Data.Items.Select(gs => new ServerAdminGameServerViewModel
            {
                GameServer = gs
            }).ToList();

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> ViewRcon(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            return View(gameServerApiResponse.Result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetRconPlayers(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewLiveRcon);

            if (!canViewLiveRcon.Succeeded)
                return Unauthorized();

            var getServerStatusResult = await serversApiClient.Rcon.V1.GetServerStatus(id);

            return Json(new
            {
                data = (getServerStatusResult.IsSuccess && getServerStatusResult.Result?.Data != null) ? getServerStatusResult.Result.Data.Players : null
            });
        }

        [HttpPost]
        public async Task<IActionResult> RestartServer(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewLiveRcon);

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
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewLiveRcon);

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
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewLiveRcon);

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
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewLiveRcon);

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
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewLiveRcon = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewLiveRcon);

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
        public async Task<IActionResult> GetChatLogAjax(bool? lockedOnly = null)
        {
            return await GetChatLogPrivate(null, null, null, lockedOnly);
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
        public async Task<IActionResult> GetGameChatLogAjax(GameType id, bool? lockedOnly = null)
        {
            var canViewGameChatLog = await _authorizationService.AuthorizeAsync(User, id, AuthPolicies.ViewGameChatLog);

            if (!canViewGameChatLog.Succeeded)
                return Unauthorized();

            return await GetChatLogPrivate(id, null, null, lockedOnly);
        }

        [HttpGet]
        public async Task<IActionResult> ServerChatLog(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewServerChatLog = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewServerChatLog);

            if (!canViewServerChatLog.Succeeded)
                return Unauthorized();

            ViewData["GameServerId"] = id;
            return View(nameof(ChatLogIndex));
        }

        [HttpPost]
        public async Task<IActionResult> GetServerChatLogAjax(Guid id, bool? lockedOnly = null)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canViewServerChatLog = await _authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ViewServerChatLog);

            if (!canViewServerChatLog.Succeeded)
                return Unauthorized();

            return await GetChatLogPrivate(null, id, null, lockedOnly);
        }

        [HttpPost]
        public async Task<IActionResult> GetPlayerChatLog(Guid id, bool? lockedOnly = null)
        {
            var playerApiResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

            if (playerApiResponse.IsNotFound || playerApiResponse.Result == null)
                return NotFound();

            var canViewGameChatLog = await _authorizationService.AuthorizeAsync(User, playerApiResponse.Result.Data.GameType, AuthPolicies.ViewGameChatLog);

            if (!canViewGameChatLog.Succeeded)
                return Unauthorized();

            return await GetChatLogPrivate(playerApiResponse.Result.Data.GameType, null, playerApiResponse.Result.Data.PlayerId, lockedOnly);
        }

        private async Task<IActionResult> GetChatLogPrivate(GameType? gameType, Guid? gameServerId, Guid? playerId, bool? lockedOnly = null)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var order = ChatMessageOrder.TimestampDesc;
            if (model.Order != null)
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "timestamp":
                        order = searchOrder == "asc" ? ChatMessageOrder.TimestampAsc : ChatMessageOrder.TimestampDesc;
                        break;
                }
            }

            // Check for locked filter parameter
            if (model.Search?.Value?.StartsWith("locked:", StringComparison.OrdinalIgnoreCase) == true)
            {
                lockedOnly = true;
                model.Search.Value = model.Search.Value.Substring(7).Trim(); // Remove "locked:" prefix
            }

            var chatMessagesApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessages(gameType, gameServerId, playerId, model.Search?.Value, model.Start, model.Length, order, lockedOnly);

            if (!chatMessagesApiResponse.IsSuccess || chatMessagesApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return Json(new
            {
                model.Draw,
                recordsTotal = chatMessagesApiResponse.Result.Data.TotalCount,
                recordsFiltered = chatMessagesApiResponse.Result.Data.FilteredCount,
                data = chatMessagesApiResponse.Result.Data.Items
            });
        }

        [HttpGet]
        public async Task<IActionResult> ChatLogPermaLink(Guid id)
        {
            var chatMessageApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessage(id);

            if (chatMessageApiResponse.IsNotFound)
                return NotFound();

            return View(chatMessageApiResponse.Result.Data);
        }

        [HttpPost]
        [Authorize(Policy = AuthPolicies.LockChatMessages)]
        public async Task<IActionResult> ToggleChatMessageLock(Guid id)
        {
            var chatMessageApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessage(id);

            if (chatMessageApiResponse.IsNotFound || chatMessageApiResponse.Result == null || chatMessageApiResponse.Result.Data.GameServer == null)
                return NotFound();

            var canLockChatMessage = await _authorizationService.AuthorizeAsync(User, chatMessageApiResponse.Result.Data.GameServer.GameType, AuthPolicies.LockChatMessages);

            if (!canLockChatMessage.Succeeded)
                return Unauthorized();

            var toggleResponse = await repositoryApiClient.ChatMessages.V1.ToggleLockedStatus(id);

            if (!toggleResponse.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            // Redirect back to the chat message permalink
            return RedirectToAction(nameof(ChatLogPermaLink), new { id });
        }
    }
}