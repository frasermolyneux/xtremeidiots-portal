using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.GeoLocationApi.Client;

using Newtonsoft.Json;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class PlayersController : Controller
    {
        private readonly IGeoLocationApiClient _geoLocationClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;

        public PlayersController(
            IGeoLocationApiClient geoLocationClient,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient)
        {
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            this.repositoryApiClient = repositoryApiClient;
            this.telemetryClient = telemetryClient;
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
            return await GetPlayersAjaxPrivate(PlayersFilter.UsernameAndGuid, id);
        }

        [HttpPost]
        public async Task<IActionResult> GetIpSearchListAjax()
        {
            return await GetPlayersAjaxPrivate(PlayersFilter.IpAddress, null);
        }

        [HttpPost]
        private async Task<IActionResult> GetPlayersAjaxPrivate(PlayersFilter filter, GameType? gameType)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model == null)
                return BadRequest();

            var order = PlayersOrder.LastSeenDesc;
            if (model.Order != null)
            {
                var orderColumn = model.Columns[model.Order.First().Column].Name;
                var searchOrder = model.Order.First().Dir;

                switch (orderColumn)
                {
                    case "gameType":
                        order = searchOrder == "asc" ? PlayersOrder.GameTypeAsc : PlayersOrder.GameTypeDesc;
                        break;
                    case "username":
                        order = searchOrder == "asc" ? PlayersOrder.UsernameAsc : PlayersOrder.UsernameDesc;
                        break;
                    case "firstSeen":
                        order = searchOrder == "asc" ? PlayersOrder.FirstSeenAsc : PlayersOrder.FirstSeenDesc;
                        break;
                    case "lastSeen":
                        order = searchOrder == "asc" ? PlayersOrder.LastSeenAsc : PlayersOrder.LastSeenDesc;
                        break;
                }
            }

            var playerCollectionApiResponse = await repositoryApiClient.Players.GetPlayers(gameType, filter, model.Search?.Value, model.Start, model.Length, order, PlayerEntityOptions.None);

            if (!playerCollectionApiResponse.IsSuccess || playerCollectionApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return Json(new
            {
                model.Draw,
                recordsTotal = playerCollectionApiResponse.Result.TotalRecords,
                recordsFiltered = playerCollectionApiResponse.Result.FilteredRecords,
                data = playerCollectionApiResponse.Result.Entries
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var playerApiResponse = await repositoryApiClient.Players.GetPlayer(id, PlayerEntityOptions.Aliases | PlayerEntityOptions.IpAddresses | PlayerEntityOptions.AdminActions | PlayerEntityOptions.RelatedPlayers | PlayerEntityOptions.ProtectedNames);

            if (playerApiResponse.IsNotFound || playerApiResponse.Result == null)
                return NotFound();

            var playerDetailsViewModel = new PlayerDetailsViewModel
            {
                Player = playerApiResponse.Result
            };

            if (!string.IsNullOrWhiteSpace(playerApiResponse.Result.IpAddress))
                try
                {
                    var geoLocation = await _geoLocationClient.GeoLookup.GetGeoLocation(playerApiResponse.Result.IpAddress);

                    if (geoLocation.IsSuccess && geoLocation.Result != null)
                        playerDetailsViewModel.GeoLocation = geoLocation.Result;
                    else
                    {
                        geoLocation.Errors.ForEach(ex => telemetryClient.TrackException(new ApplicationException(ex)));
                    }
                }
                catch (Exception ex)
                {
                    telemetryClient.TrackException(ex);
                }

            return View(playerDetailsViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> MyActions()
        {
            var adminActionsApiResponse = await repositoryApiClient.AdminActions.GetAdminActions(null, null, User.XtremeIdiotsId(), null, 0, 50, AdminActionOrder.CreatedDesc);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return View(adminActionsApiResponse.Result.Entries);
        }

        [HttpGet]
        public async Task<IActionResult> Unclaimed()
        {
            var adminActionsApiResponse = await repositoryApiClient.AdminActions.GetAdminActions(null, null, null, AdminActionFilter.UnclaimedBans, 0, 50, AdminActionOrder.CreatedDesc);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return View(adminActionsApiResponse.Result.Entries);
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
            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.GetCumulativeDailyPlayers(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return Json(playerAnalyticsResponse.Result.Entries);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewDailyPlayersPerGameJson(DateTime cutoff)
        {
            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.GetNewDailyPlayersPerGame(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return Json(playerAnalyticsResponse.Result.Entries);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.GetPlayersDropOffPerGameJson(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return Json(playerAnalyticsResponse.Result.Entries);
        }

        #region Protected Names

        [HttpGet]
        public async Task<IActionResult> ProtectedNames()
        {
            var protectedNamesResponse = await repositoryApiClient.Players.GetProtectedNames(0, 1000);

            if (!protectedNamesResponse.IsSuccess || protectedNamesResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var model = new ProtectedNamesViewModel
            {
                ProtectedNames = protectedNamesResponse.Result.Entries
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddProtectedName(Guid id)
        {
            var playerResponse = await repositoryApiClient.Players.GetPlayer(id, PlayerEntityOptions.None);

            if (!playerResponse.IsSuccess || playerResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            var model = new CreateProtectedNameViewModel(id)
            {
                Player = playerResponse.Result
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProtectedName(CreateProtectedNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var playerResponse = await repositoryApiClient.Players.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                if (playerResponse.IsSuccess && playerResponse.Result != null)
                {
                    model.Player = playerResponse.Result;
                }
                return View(model);
            }

            var createProtectedNameDto = new CreateProtectedNameDto(
                model.PlayerId,
                model.Name,
                User.UserProfileId());

            var response = await repositoryApiClient.Players.CreateProtectedName(createProtectedNameDto);

            if (!response.IsSuccess)
            {
                if (response.IsConflict)
                {
                    ModelState.AddModelError("Name", "This name is already protected by another player");

                    var playerResponse = await repositoryApiClient.Players.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                    if (playerResponse.IsSuccess && playerResponse.Result != null)
                    {
                        model.Player = playerResponse.Result;
                    }

                    return View(model);
                }

                return RedirectToAction("Display", "Errors", new { id = 500 });
            }

            return RedirectToAction(nameof(Details), new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProtectedName(Guid id)
        {
            var protectedNameResponse = await repositoryApiClient.Players.GetProtectedName(id);

            if (!protectedNameResponse.IsSuccess || protectedNameResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            var deleteProtectedNameDto = new DeleteProtectedNameDto(id);
            var response = await repositoryApiClient.Players.DeleteProtectedName(deleteProtectedNameDto);

            if (!response.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return RedirectToAction(nameof(Details), new { id = protectedNameResponse.Result.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> ProtectedNameReport(Guid id)
        {
            var reportResponse = await repositoryApiClient.Players.GetProtectedNameUsageReport(id);

            if (!reportResponse.IsSuccess || reportResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            var model = new ProtectedNameReportViewModel
            {
                Report = reportResponse.Result
            };

            return View(model);
        }

        #endregion
    }
}