using System;
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
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;
using XI.Portal.Web.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.PlayersManagement)]
    public class PlayersController : Controller
    {
        private readonly IGeoLocationClient _geoLocationClient;
        private readonly IPlayersRepository _playersRepository;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator};

        public PlayersController(IPlayersRepository playersRepository, IGeoLocationClient geoLocationClient)
        {
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
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
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var player = await _playersRepository.GetPlayer((Guid) id, User, _requiredClaims);

            var playerDetailsViewModel = new PlayerDetailsViewModel
            {
                Player = player
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


            //try
            //{
            //    if (!Guid.TryParse(id, out var idAsGuid))
            //        return RedirectToAction("Home");

            //    using (var context = ContextProvider.GetContext())
            //    {
            //        var player = await context.Players.SingleOrDefaultAsync(p => p.PlayerId == idAsGuid);

            //        if (player == null)
            //            return RedirectToAction("Home");

            //        var model = new PlayerInfoViewModel
            //        {
            //            Player = player,
            //            Aliases = await context.PlayerAliases.Where(pa => pa.Player.PlayerId == player.PlayerId)
            //                .OrderByDescending(pa => pa.LastUsed).ToListAsync(),
            //            IpAddresses = await context.PlayerIpAddresses
            //                .Where(pip => pip.Player.PlayerId == player.PlayerId)
            //                .OrderByDescending(pip => pip.LastUsed).ToListAsync(),
            //            AdminActions = await context.AdminActions.Include(aa => aa.Admin)
            //                .Where(aa => aa.Player.PlayerId == player.PlayerId)
            //                .OrderByDescending(aa => aa.Created).ToListAsync(),
            //            RelatedIpAddresses = new List<PlayerIpAddress>()
            //        };

            //        foreach (var playerIpAddress in model.IpAddresses)
            //        {
            //            var relatedPlayersFromIp = await context.PlayerIpAddresses.Include(ip => ip.Player)
            //                .Where(ip => ip.Address == playerIpAddress.Address && ip.Player.PlayerId != idAsGuid)
            //                .ToListAsync();
            //            model.RelatedIpAddresses.AddRange(relatedPlayersFromIp);
            //        }

            //        LookupAddressResponse lookupAddressResponse = null;
            //        try
            //        {
            //            lookupAddressResponse = await geoLocationClient.LookupAddress(player.IpAddress);
            //        }
            //        catch (Exception)
            //        {
            //            // swallow
            //        }

            //        if (lookupAddressResponse != null)
            //            model.LookupAddressResponse = lookupAddressResponse;

            //        return View(model);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    await DatabaseLogger.CreateSystemLogAsync("Error", "[Portal] Unhandled Error", ex);
            //    throw;
            //}
        }

        public class PlayerDetailsViewModel
        {
            public PlayerDto Player { get; set; }
            public GeoLocationDto GeoLocation { get; set; }
        }
    }
}