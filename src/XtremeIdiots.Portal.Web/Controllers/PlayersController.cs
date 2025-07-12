using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using MX.GeoLocation.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
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
            var playerCollectionApiResponse = await repositoryApiClient.Players.V1.GetPlayers(gameType, filter, model.Search?.Value, model.Start, model.Length, order, PlayerEntityOptions.None);

            if (!playerCollectionApiResponse.IsSuccess || playerCollectionApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });            // Enrich player data with ProxyCheck information
            var enrichedPlayers = await playerCollectionApiResponse.Result.Data.Items.EnrichWithProxyCheckDataAsync(_proxyCheckService, _logger);

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
                recordsTotal = playerCollectionApiResponse.Result.Data.TotalCount,
                recordsFiltered = playerCollectionApiResponse.Result.Data.FilteredCount,
                data = playerData
            });
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var playerApiResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.Aliases | PlayerEntityOptions.IpAddresses | PlayerEntityOptions.AdminActions | PlayerEntityOptions.RelatedPlayers | PlayerEntityOptions.ProtectedNames | PlayerEntityOptions.Tags);

            if (playerApiResponse.IsNotFound || playerApiResponse.Result == null)
                return NotFound();

            var playerDetailsViewModel = new PlayerDetailsViewModel
            {
                Player = playerApiResponse.Result.Data
            };

            // Enrich the current player IP with geolocation data (legacy behavior)
            if (!string.IsNullOrWhiteSpace(playerApiResponse.Result.Data.IpAddress))
                try
                {
                    var getGeoLocationResult = await _geoLocationClient.GeoLookup.V1.GetGeoLocation(playerApiResponse.Result.Data.IpAddress);

                    if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result?.Data != null)
                        playerDetailsViewModel.GeoLocation = getGeoLocationResult.Result.Data;
                }
                catch (Exception ex)
                {
                    telemetryClient.TrackException(ex);
                }

            // Enrich all IP addresses with geolocation and proxy check data
            if (playerApiResponse.Result.Data.PlayerIpAddresses != null && playerApiResponse.Result.Data.PlayerIpAddresses.Any())
            {
                foreach (var ipAddress in playerApiResponse.Result.Data.PlayerIpAddresses)
                {
                    var enrichedIp = new PlayerIpAddressViewModel
                    {
                        IpAddressDto = ipAddress,
                        IsCurrentIp = ipAddress.Address == playerApiResponse.Result.Data.IpAddress
                    };

                    try
                    {
                        // Get geolocation data
                        var getGeoLocationResult = await _geoLocationClient.GeoLookup.V1.GetGeoLocation(ipAddress.Address);
                        if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result != null)
                        {
                            enrichedIp.GeoLocation = getGeoLocationResult.Result.Data;
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
            var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, User.XtremeIdiotsId(), null, 0, 50, AdminActionOrder.CreatedDesc);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return View(adminActionsApiResponse.Result.Data.Items);
        }

        [HttpGet]
        public async Task<IActionResult> Unclaimed()
        {
            var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, null, AdminActionFilter.UnclaimedBans, 0, 50, AdminActionOrder.CreatedDesc);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return View(adminActionsApiResponse.Result.Data.Items);
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
            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetCumulativeDailyPlayers(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return Json(playerAnalyticsResponse.Result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewDailyPlayersPerGameJson(DateTime cutoff)
        {
            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetNewDailyPlayersPerGame(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return Json(playerAnalyticsResponse.Result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetPlayersDropOffPerGameJson(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return Json(playerAnalyticsResponse.Result.Data);
        }

        #region Protected Names

        [HttpGet]
        public async Task<IActionResult> ProtectedNames()
        {
            var protectedNamesResponse = await repositoryApiClient.Players.V1.GetProtectedNames(0, 1000);

            if (!protectedNamesResponse.IsSuccess || protectedNamesResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var model = new ProtectedNamesViewModel
            {
                ProtectedNames = protectedNamesResponse.Result.Data.Items.ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddProtectedName(Guid id)
        {
            var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

            if (!playerResponse.IsSuccess || playerResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            var model = new CreateProtectedNameViewModel(id)
            {
                Player = playerResponse.Result.Data
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProtectedName(CreateProtectedNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                if (playerResponse.IsSuccess && playerResponse.Result != null)
                {
                    model.Player = playerResponse.Result.Data;
                }
                return View(model);
            }

            var createProtectedNameDto = new CreateProtectedNameDto(
                model.PlayerId,
                model.Name,
                User.XtremeIdiotsId());

            var response = await repositoryApiClient.Players.V1.CreateProtectedName(createProtectedNameDto);

            if (!response.IsSuccess)
            {
                if (response.IsConflict)
                {
                    ModelState.AddModelError("Name", "This name is already protected by another player");

                    var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                    if (playerResponse.IsSuccess && playerResponse.Result != null)
                    {
                        model.Player = playerResponse.Result.Data;
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
            var protectedNameResponse = await repositoryApiClient.Players.V1.GetProtectedName(id);

            if (!protectedNameResponse.IsSuccess || protectedNameResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            var deleteProtectedNameDto = new DeleteProtectedNameDto(id);
            var response = await repositoryApiClient.Players.V1.DeleteProtectedName(deleteProtectedNameDto);

            if (!response.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return RedirectToAction(nameof(Details), new { id = protectedNameResponse.Result.Data.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> ProtectedNameReport(Guid id)
        {
            var reportResponse = await repositoryApiClient.Players.V1.GetProtectedNameUsageReport(id);

            if (!reportResponse.IsSuccess || reportResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            var model = new ProtectedNameReportViewModel
            {
                Report = reportResponse.Result.Data
            };

            return View(model);
        }

        #endregion

        #region Player Tags

        [HttpGet]
        [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
        public async Task<IActionResult> AddPlayerTag(Guid id)
        {
            var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

            if (playerResponse.IsNotFound || playerResponse.Result == null)
                return NotFound();

            var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);

            if (!tagsResponse.IsSuccess || tagsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 }); var model = new AddPlayerTagViewModel
                {
                    PlayerId = id,
                    Player = playerResponse.Result.Data,
                    AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList()
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
                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                if (playerResponse.IsSuccess && playerResponse.Result != null)
                {
                    model.Player = playerResponse.Result.Data;
                }
                var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);
                if (tagsResponse.IsSuccess && tagsResponse.Result != null)
                {
                    model.AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList();
                }
                return View(model);
            }
            var tagResponse = await repositoryApiClient.Tags.V1.GetTag(model.TagId);
            if (!tagResponse.IsSuccess || tagResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            // Check if this tag is UserDefined - only those can be added by users
            if (!tagResponse.Result.Data.UserDefined)
            {
                this.AddAlertDanger("This tag cannot be assigned to players as it is not marked as User Defined.");

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                if (playerResponse.IsSuccess && playerResponse.Result != null)
                {
                    model.Player = playerResponse.Result.Data;
                }

                var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);
                if (tagsResponse.IsSuccess && tagsResponse.Result != null)
                {
                    model.AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList();
                }

                return View(model);
            }

            var userProfileIdString = User.UserProfileId();
            if (string.IsNullOrWhiteSpace(userProfileIdString) || !Guid.TryParse(userProfileIdString, out var userProfileId))
            {
                return RedirectToAction("Display", "Errors", new { id = 400 });
            }

            var playerTagDto = new PlayerTagDto
            {
                PlayerId = model.PlayerId,
                TagId = model.TagId,
                UserProfileId = userProfileId,
                Assigned = DateTime.UtcNow
            };

            var response = await repositoryApiClient.Players.V1.AddPlayerTag(model.PlayerId, playerTagDto);

            if (!response.IsSuccess)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var eventTelemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("PlayerTagAdded").Enrich(User);
            eventTelemetry.Properties.Add("PlayerId", model.PlayerId.ToString());
            eventTelemetry.Properties.Add("TagId", model.TagId.ToString());
            eventTelemetry.Properties.Add("TagName", tagResponse.Result.Data.Name);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The tag '{tagResponse.Result.Data.Name}' has been successfully added to the player");

            return RedirectToAction(nameof(Details), new { id = model.PlayerId });
        }
        [HttpGet]
        [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
        public async Task<IActionResult> RemovePlayerTag(Guid id, Guid playerTagId)
        {
            var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);
            if (playerResponse.IsNotFound || playerResponse.Result == null)
                return NotFound();

            var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(id);
            if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var playerTag = playerTagsResponse.Result.Data.Items.FirstOrDefault(pt => pt.PlayerTagId == playerTagId);
            if (playerTag == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            // Check if the tag is UserDefined - only those can be removed
            if (playerTag.Tag != null && !playerTag.Tag.UserDefined)
            {
                this.AddAlertDanger("This tag cannot be removed as it is not marked as User Defined.");
                return RedirectToAction(nameof(Details), new { id = id });
            }

            ViewBag.Player = playerResponse.Result.Data;
            return View(playerTag);
        }
        [HttpPost]
        [ActionName("RemovePlayerTag")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
        public async Task<IActionResult> RemovePlayerTagConfirmed(Guid id, Guid playerTagId)
        {
            var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);
            if (playerResponse.IsNotFound || playerResponse.Result == null)
                return NotFound();

            var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(id);
            if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var playerTag = playerTagsResponse.Result.Data.Items.FirstOrDefault(pt => pt.PlayerTagId == playerTagId);
            if (playerTag == null)
                return RedirectToAction("Display", "Errors", new { id = 404 });

            // Check if the tag is UserDefined - only those can be removed
            if (playerTag.Tag != null && !playerTag.Tag.UserDefined)
            {
                this.AddAlertDanger("This tag cannot be removed as it is not marked as User Defined.");
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var response = await repositoryApiClient.Players.V1.RemovePlayerTag(id, playerTagId);

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