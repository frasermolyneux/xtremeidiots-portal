using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MX.GeoLocation.GeoLocationApi.Client;

using Newtonsoft.Json;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.AdminWebApp.Services;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Tags;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class PlayersController : Controller
    {
        private readonly IGeoLocationApiClient _geoLocationClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly IProxyCheckService _proxyCheckService;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(
            IGeoLocationApiClient geoLocationClient,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            IProxyCheckService proxyCheckService,
            ILogger<PlayersController> logger)
        {
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _proxyCheckService = proxyCheckService ?? throw new ArgumentNullException(nameof(proxyCheckService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                return RedirectToAction("Display", "Errors", new { id = 500 });            // Enrich player data with ProxyCheck information
            var enrichedPlayers = await playerCollectionApiResponse.Result.Entries.EnrichWithProxyCheckDataAsync(_proxyCheckService, _logger);

            // Convert the player DTOs to dynamic objects that include ProxyCheck data
            var playerData = enrichedPlayers.Select(player => new
            {
                player.PlayerId,
                player.GameType,
                player.Username,
                player.Guid,
                player.IpAddress,
                player.FirstSeen,
                player.LastSeen,
                // Add ProxyCheck data
                ProxyCheckRiskScore = player.ProxyCheckRiskScore(),
                IsProxy = player.IsProxy(),
                IsVpn = player.IsVpn()
            }).ToList();

            return Json(new
            {
                model.Draw,
                recordsTotal = playerCollectionApiResponse.Result.TotalRecords,
                recordsFiltered = playerCollectionApiResponse.Result.FilteredRecords,
                data = playerData
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

            // Enrich the current player IP with geolocation data (legacy behavior)
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

            // Enrich all IP addresses with geolocation and proxy check data
            if (playerApiResponse.Result.PlayerIpAddresses != null && playerApiResponse.Result.PlayerIpAddresses.Any())
            {
                foreach (var ipAddress in playerApiResponse.Result.PlayerIpAddresses)
                {
                    var enrichedIp = new PlayerIpAddressViewModel
                    {
                        IpAddressDto = ipAddress,
                        IsCurrentIp = ipAddress.Address == playerApiResponse.Result.IpAddress
                    };

                    try
                    {
                        // Get geolocation data
                        var geoLocation = await _geoLocationClient.GeoLookup.GetGeoLocation(ipAddress.Address);
                        if (geoLocation.IsSuccess && geoLocation.Result != null)
                        {
                            enrichedIp.GeoLocation = geoLocation.Result;
                        }

                        // Get proxy check data
                        var proxyCheck = await _proxyCheckService.GetIpRiskDataAsync(ipAddress.Address);
                        if (!proxyCheck.IsError)
                        {
                            enrichedIp.ProxyCheck = proxyCheck;
                        }
                    }
                    catch (Exception ex)
                    {
                        telemetryClient.TrackException(ex);
                    }

                    playerDetailsViewModel.EnrichedIpAddresses.Add(enrichedIp);
                }
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
                User.XtremeIdiotsId());

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

        #region Player Tags

        [HttpGet]
        [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
        public async Task<IActionResult> AddPlayerTag(Guid id)
        {
            var playerResponse = await repositoryApiClient.Players.GetPlayer(id, PlayerEntityOptions.None);

            if (playerResponse.IsNotFound || playerResponse.Result == null)
                return NotFound();

            var tagsResponse = await repositoryApiClient.Tags.GetTags(0, 100);

            if (!tagsResponse.IsSuccess || tagsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var model = new AddPlayerTagViewModel
            {
                PlayerId = id,
                Player = playerResponse.Result,
                AvailableTags = tagsResponse.Result.Entries
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
        public async Task<IActionResult> AddPlayerTag(AddPlayerTagViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var playerResponse = await repositoryApiClient.Players.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                if (playerResponse.IsSuccess && playerResponse.Result != null)
                {
                    model.Player = playerResponse.Result;
                }

                var tagsResponse = await repositoryApiClient.Tags.GetTags(0, 100);
                if (tagsResponse.IsSuccess && tagsResponse.Result != null)
                {
                    model.AvailableTags = tagsResponse.Result.Entries;
                }

                return View(model);
            }

            var tagResponse = await repositoryApiClient.Tags.GetTag(model.TagId);
            if (!tagResponse.IsSuccess || tagResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            var playerTagDto = new PlayerTagDto
            {
                PlayerId = model.PlayerId,
                TagId = model.TagId,
                UserProfileId = new Guid(User.XtremeIdiotsId()),
                Assigned = DateTime.UtcNow
            };

            var response = await repositoryApiClient.Players.AddPlayerTag(model.PlayerId, playerTagDto);

            if (!response.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var eventTelemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("PlayerTagAdded").Enrich(User);
            eventTelemetry.Properties.Add("PlayerId", model.PlayerId.ToString());
            eventTelemetry.Properties.Add("TagId", model.TagId.ToString());
            eventTelemetry.Properties.Add("TagName", tagResponse.Result.Name);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The tag '{tagResponse.Result.Name}' has been successfully added to the player");

            return RedirectToAction(nameof(Details), new { id = model.PlayerId });
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
        public async Task<IActionResult> RemovePlayerTag(Guid id, Guid playerTagId)
        {
            var playerResponse = await repositoryApiClient.Players.GetPlayer(id, PlayerEntityOptions.None);
            if (playerResponse.IsNotFound || playerResponse.Result == null)
                return NotFound();

            var playerTagsResponse = await repositoryApiClient.Players.GetPlayerTags(id);
            if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var playerTag = playerTagsResponse.Result.Entries.FirstOrDefault(pt => pt.PlayerTagId == playerTagId);
            if (playerTag == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            ViewBag.Player = playerResponse.Result;
            return View(playerTag);
        }

        [HttpPost]
        [ActionName("RemovePlayerTag")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
        public async Task<IActionResult> RemovePlayerTagConfirmed(Guid id, Guid playerTagId)
        {
            var playerResponse = await repositoryApiClient.Players.GetPlayer(id, PlayerEntityOptions.None);
            if (playerResponse.IsNotFound || playerResponse.Result == null)
                return NotFound();

            var playerTagsResponse = await repositoryApiClient.Players.GetPlayerTags(id);
            if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var playerTag = playerTagsResponse.Result.Entries.FirstOrDefault(pt => pt.PlayerTagId == playerTagId);
            if (playerTag == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            var response = await repositoryApiClient.Players.RemovePlayerTag(id, playerTagId);

            if (!response.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var eventTelemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("PlayerTagRemoved").Enrich(User);
            eventTelemetry.Properties.Add("PlayerId", id.ToString());
            eventTelemetry.Properties.Add("PlayerTagId", playerTagId.ToString());
            eventTelemetry.Properties.Add("TagName", playerTag.Tag?.Name ?? "Unknown");
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The tag '{playerTag.Tag?.Name ?? "Unknown"}' has been successfully removed from the player");

            return RedirectToAction(nameof(Details), new { id = id });
        }

        #endregion
    }
}