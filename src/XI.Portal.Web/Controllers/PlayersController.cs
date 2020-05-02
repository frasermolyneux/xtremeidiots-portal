using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Interfaces;
using FM.GeoLocation.Contract.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;
using XI.Portal.Web.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.AccessPlayers)]
    public class PlayersController : Controller
    {
        private readonly IAdminActionsRepository _adminActionsRepository;
        private readonly IGeoLocationClient _geoLocationClient;
        private readonly IPlayersRepository _playersRepository;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator};

        public PlayersController(IPlayersRepository playersRepository,
            IGeoLocationClient geoLocationClient,
            IAdminActionsRepository adminActionsRepository)
        {
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
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
                filterModel.GameType = (GameType) id;

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

            var player = await _playersRepository.GetPlayer((Guid) id);

            var adminActionsFilterModel = new AdminActionsFilterModel
            {
                PlayerId = (Guid) id,
                Order = AdminActionsFilterModel.OrderBy.CreatedDesc
            };

            var adminActions = await _adminActionsRepository.GetAdminActions(adminActionsFilterModel);

            var playerDetailsViewModel = new PlayerDetailsViewModel
            {
                Player = player,
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

            var aliases = await _playersRepository.GetPlayerAliases((Guid) id, User, _requiredClaims);

            return Json(new
            {
                data = aliases
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayerIpAddressesAjax(Guid? id)
        {
            if (id == null) return NotFound();

            var ipAddresses = await _playersRepository.GetPlayerIpAddresses((Guid) id, User, _requiredClaims);

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

            var relatedPlayers = await _playersRepository.GetRelatedPlayers((Guid) id, ipAddress, User, _requiredClaims);

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

        public class PlayerDetailsViewModel
        {
            public PlayerDto Player { get; set; }
            public GeoLocationDto GeoLocation { get; set; }
            public List<AdminActionDto> AdminActions { get; set; }
        }
    }
}