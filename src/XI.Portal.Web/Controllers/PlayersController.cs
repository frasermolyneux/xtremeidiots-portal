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
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class PlayersController : Controller
    {
        private readonly IAdminActionsRepository _adminActionsRepository;
        private readonly IPlayerAnalyticsRepository _playerAnalyticsRepository;
        private readonly IGeoLocationClient _geoLocationClient;
        private readonly IPlayersRepository _playersRepository;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;

        public PlayersController(
            IPlayersRepository playersRepository,
            IGeoLocationClient geoLocationClient,
            IAdminActionsRepository adminActionsRepository,
            IPlayerAnalyticsRepository playerAnalyticsRepository,
            IRepositoryApiClient repositoryApiClient,
            IRepositoryTokenProvider repositoryTokenProvider)
        {
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
            _playerAnalyticsRepository = playerAnalyticsRepository ?? throw new ArgumentNullException(nameof(playerAnalyticsRepository));
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
            return await GetPlayersAjaxPrivate(PlayersFilterModel.FilterType.UsernameAndGuid, id);
        }

        [HttpPost]
        public async Task<IActionResult> GetIpSearchListAjax()
        {
            return await GetPlayersAjaxPrivate(PlayersFilterModel.FilterType.IpAddress, null);
        }

        [HttpPost]
        private async Task<IActionResult> GetPlayersAjaxPrivate(PlayersFilterModel.FilterType filterType, GameType? id)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var filterModel = new PlayersFilterModel
            {
                Filter = filterType
            };

            if (id != null)
                filterModel.GameType = (GameType)id;

            var recordsTotal = await _playersRepository.GetPlayerListCount(filterModel);

            filterModel.FilterString = model.Search?.Value;
            var recordsFiltered = await _playersRepository.GetPlayerListCount(filterModel);

            filterModel.TakeEntries = model.Length;
            filterModel.SkipEntries = model.Start;

            if (model.Order == null)
            {
                filterModel.Order = PlayersFilterModel.OrderBy.LastSeenDesc;
            }
            else
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "gameType":
                        filterModel.Order = searchOrder == "asc" ? PlayersFilterModel.OrderBy.GameTypeAsc : PlayersFilterModel.OrderBy.GameTypeDesc;
                        break;
                    case "username":
                        filterModel.Order = searchOrder == "asc" ? PlayersFilterModel.OrderBy.UsernameAsc : PlayersFilterModel.OrderBy.UsernameDesc;
                        break;
                    case "firstSeen":
                        filterModel.Order = searchOrder == "asc" ? PlayersFilterModel.OrderBy.FirstSeenAsc : PlayersFilterModel.OrderBy.FirstSeenDesc;
                        break;
                    case "lastSeen":
                        filterModel.Order = searchOrder == "asc" ? PlayersFilterModel.OrderBy.LastSeenAsc : PlayersFilterModel.OrderBy.LastSeenDesc;
                        break;
                }
            }

            var playersListEntries = await _playersRepository.GetPlayerList(filterModel);

            return Json(new
            {
                model.Draw,
                recordsTotal,
                recordsFiltered,
                data = playersListEntries
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var player = await repositoryApiClient.PlayersApiClient.GetPlayer(accessToken, (Guid)id);

            var adminActionsFilterModel = new AdminActionsFilterModel
            {
                PlayerId = (Guid)id,
                Order = AdminActionsFilterModel.OrderBy.CreatedDesc
            };

            var adminActions = await _adminActionsRepository.GetAdminActions(adminActionsFilterModel);

            var playerDetailsViewModel = new PlayerDetailsViewModel
            {
                Player = new PlayerDto
                {
                    PlayerId = player.Id,
                    GameType = Enum.Parse<GameType>(player.GameType),
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
            var aliases = await repositoryApiClient.PlayersApiClient.GetPlayerAliases(accessToken, (Guid)id);

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
            var ipAddresses = await repositoryApiClient.PlayersApiClient.GetPlayerIpAddresses(accessToken, (Guid)id);

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
            var relatedPlayers = await repositoryApiClient.PlayersApiClient.GetRelatedPlayers(accessToken, (Guid)id, ipAddress);

            return Json(new
            {
                data = relatedPlayers
            });
        }

        [HttpGet]
        public async Task<IActionResult> MyActions()
        {
            var filterModel = new AdminActionsFilterModel
            {
                AdminId = User.XtremeIdiotsId(),
                Order = AdminActionsFilterModel.OrderBy.CreatedDesc
            };

            var adminActions = await _adminActionsRepository.GetAdminActions(filterModel);

            return View(adminActions);
        }

        [HttpGet]
        public async Task<IActionResult> Unclaimed()
        {
            var filterModel = new AdminActionsFilterModel
            {
                Filter = AdminActionsFilterModel.FilterType.UnclaimedBans,
                Order = AdminActionsFilterModel.OrderBy.CreatedDesc
            };

            var adminActions = await _adminActionsRepository.GetAdminActions(filterModel);

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
            var data = await _playerAnalyticsRepository.GetCumulativeDailyPlayers(cutoff);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewDailyPlayersPerGameJson(DateTime cutoff)
        {
            var data = await _playerAnalyticsRepository.GetNewDailyPlayersPerGame(cutoff);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var data = await _playerAnalyticsRepository.GetPlayersDropOffPerGameJson(cutoff);
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