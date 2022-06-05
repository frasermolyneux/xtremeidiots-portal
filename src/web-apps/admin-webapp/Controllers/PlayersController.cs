﻿using FM.GeoLocation.Contract.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class PlayersController : Controller
    {
        private readonly IGeoLocationClient _geoLocationClient;
        private readonly IRepositoryApiClient repositoryApiClient;

        public PlayersController(
            IGeoLocationClient geoLocationClient,
            IRepositoryApiClient repositoryApiClient)
        {
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            this.repositoryApiClient = repositoryApiClient;
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
        private async Task<IActionResult> GetPlayersAjaxPrivate(string filter, GameType? id)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var order = "LastSeenDesc";
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

            var searchResponse = await repositoryApiClient.Players.SearchPlayers(id.ToString(), filter, model.Search?.Value, model.Length, model.Start, order);

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

            var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayer((Guid)id);
            var adminActions = await repositoryApiClient.Players.GetAdminActionsForPlayer((Guid)id);

            var playerDetailsViewModel = new PlayerDetailsViewModel
            {
                Player = playerDtoApiResponse.Result,
                AdminActions = adminActions
            };

            if (!string.IsNullOrWhiteSpace(playerDtoApiResponse.Result.IpAddress))
                try
                {
                    var geoLocation = await _geoLocationClient.LookupAddress(playerDtoApiResponse.Result.IpAddress);

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
        public async Task<IActionResult> MyActions()
        {
            var adminActions = await repositoryApiClient.AdminActions.GetAdminActions(null, null, User.XtremeIdiotsId(), null, 0, 0, AdminActionOrder.CreatedDesc);

            return View(adminActions);
        }

        [HttpGet]
        public async Task<IActionResult> Unclaimed()
        {
            var adminActions = await repositoryApiClient.AdminActions.GetAdminActions(null, null, null, AdminActionFilter.UnclaimedBans, 0, 0, AdminActionOrder.CreatedDesc);

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
            var data = await repositoryApiClient.PlayerAnalytics.GetCumulativeDailyPlayers(cutoff);

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewDailyPlayersPerGameJson(DateTime cutoff)
        {
            var data = await repositoryApiClient.PlayerAnalytics.GetNewDailyPlayersPerGame(cutoff);

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var data = await repositoryApiClient.PlayerAnalytics.GetPlayersDropOffPerGameJson(cutoff);

            return Json(data);
        }
    }
}