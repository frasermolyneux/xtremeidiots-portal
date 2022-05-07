using FM.GeoLocation.Contract.Interfaces;
using FM.GeoLocation.Contract.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class PlayersController : Controller
    {
        private readonly IGeoLocationClient _geoLocationClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;

        public PlayersController(
            IGeoLocationClient geoLocationClient,
            IRepositoryApiClient repositoryApiClient,
            IRepositoryTokenProvider repositoryTokenProvider)
        {
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            this.repositoryApiClient = repositoryApiClient;
            this.repositoryTokenProvider = repositoryTokenProvider;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GameIndex(GameType? id)
        {
            ViewData["GameType"] = id;
            return View(nameof(Index));
        }

        [HttpGet]
        public IActionResult IpIndex()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPlayersAjax(GameType? id)
        {
            return await GetPlayersAjaxPrivate("UsernameAndGuid", id);
        }

        [HttpPost]
        public async Task<IActionResult> GetIpSearchListAjax()
        {
            return await GetPlayersAjaxPrivate("IpAddress", null);
        }

        [HttpPost]
        private async Task<IActionResult> GetPlayersAjaxPrivate(string filterType, GameType? id)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            string order = "LastSeenDesc";
            if (model.Order != null)
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "gameType":
                        order = searchOrder == "asc" ? "GameTypeAsc" : "GameTypeDesc";
                        break;
                    case "username":
                        order = searchOrder == "asc" ? "UsernameAsc" : "UsernameDesc";
                        break;
                    case "firstSeen":
                        order = searchOrder == "asc" ? "FirstSeenAsc" : "FirstSeenDesc";
                        break;
                    case "lastSeen":
                        order = searchOrder == "asc" ? "LastSeenAsc" : "LastSeenDesc";
                        break;
                }
            }

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            PlayersSearchResponseDto searchResponse = await repositoryApiClient.Players.SearchPlayers(accessToken, id.ToString(), filterType, model.Search?.Value, model.Length, model.Start, order);

            return Json(new
            {
                model.Draw,
                recordsTotal = searchResponse.TotalRecords,
                recordsFiltered = searchResponse.FilteredRecords,
                data = searchResponse.Entries
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var player = await repositoryApiClient.Players.GetPlayer(accessToken, (Guid)id);
            var adminActions = await repositoryApiClient.Players.GetAdminActionsForPlayer(accessToken, (Guid)id);

            var playerDetailsViewModel = new PlayerDetailsViewModel
            {
                Player = new PlayerDto
                {
                    Id = player.Id,
                    GameType = player.GameType,
                    Username = player.Username,
                    Guid = player.Guid,
                    IpAddress = player.IpAddress,
                    FirstSeen = player.FirstSeen,
                    LastSeen = player.LastSeen
                },
                AdminActions = adminActions
            };

            if (!string.IsNullOrWhiteSpace(player.IpAddress))
                try
                {
                    var geoLocation = await _geoLocationClient.LookupAddress(player.IpAddress);

                    if (geoLocation.Success)
                        playerDetailsViewModel.GeoLocation = geoLocation.GeoLocationDto;
                }
                catch
                {
                    // ignored
                }

            return View(playerDetailsViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayerAliasesAjax(Guid? id)
        {
            if (id == null) return NotFound();

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var aliases = await repositoryApiClient.Players.GetPlayerAliases(accessToken, (Guid)id);

            return Json(new
            {
                data = aliases
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayerIpAddressesAjax(Guid? id)
        {
            if (id == null) return NotFound();

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var ipAddresses = await repositoryApiClient.Players.GetPlayerIpAddresses(accessToken, (Guid)id);

            return Json(new
            {
                data = ipAddresses
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetRelatedPlayersAjax(Guid? id, string ipAddress)
        {
            if (id == null) return NotFound();

            if (string.IsNullOrWhiteSpace(ipAddress))
                return BadRequest();

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var relatedPlayers = await repositoryApiClient.Players.GetRelatedPlayers(accessToken, (Guid)id, ipAddress);

            return Json(new
            {
                data = relatedPlayers
            });
        }

        [HttpGet]
        public async Task<IActionResult> MyActions()
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActions = await repositoryApiClient.AdminActions.GetAdminActions(accessToken, null, null, User.XtremeIdiotsId(), null, 0, 0, "CreatedDesc");

            return View(adminActions);
        }

        [HttpGet]
        public async Task<IActionResult> Unclaimed()
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActions = await repositoryApiClient.AdminActions.GetAdminActions(accessToken, null, null, null, "UnclaimedBans", 0, 0, "CreatedDesc");

            return View(adminActions);
        }

        [HttpGet]
        public IActionResult Analytics()
        {
            var cutoff = DateTime.UtcNow.AddMonths(-3);
            ViewBag.DateFilterRange = cutoff;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCumulativeDailyPlayersJson(DateTime cutoff)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var data = await repositoryApiClient.PlayerAnalytics.GetCumulativeDailyPlayers(accessToken, cutoff);

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewDailyPlayersPerGameJson(DateTime cutoff)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var data = await repositoryApiClient.PlayerAnalytics.GetNewDailyPlayersPerGame(accessToken, cutoff);

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var data = await repositoryApiClient.PlayerAnalytics.GetPlayersDropOffPerGameJson(accessToken, cutoff);

            return Json(data);
        }

        public class PlayerDetailsViewModel
        {
            public PlayerDto Player { get; set; }
            public GeoLocationDto GeoLocation { get; set; }
            public List<AdminActionDto> AdminActions { get; set; }
        }
    }
}