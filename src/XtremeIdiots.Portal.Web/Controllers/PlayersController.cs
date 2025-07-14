using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
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
    /// <summary>
    /// Controller for managing players, player analytics, protected names, and player tags
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class PlayersController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IGeoLocationApiClient geoLocationClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly IProxyCheckService proxyCheckService;
        private readonly ILogger<PlayersController> logger;

        public PlayersController(
            IAuthorizationService authorizationService,
            IGeoLocationApiClient geoLocationClient,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            IProxyCheckService proxyCheckService,
            ILogger<PlayersController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.proxyCheckService = proxyCheckService ?? throw new ArgumentNullException(nameof(proxyCheckService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the main players index page with search capabilities
        /// </summary>
        /// <returns>The players index view</returns>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                logger.LogInformation("User {UserId} accessing players index", User.XtremeIdiotsId());
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading players index for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the players index page filtered by game type
        /// </summary>
        /// <param name="id">The game type to filter by</param>
        /// <returns>The players index view with game type filter</returns>
        [HttpGet]
        public IActionResult GameIndex(GameType? id)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing players index for game type {GameType}",
                    User.XtremeIdiotsId(), id);

                ViewData["GameType"] = id;
                return View(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading players game index for user {UserId} and game type {GameType}",
                    User.XtremeIdiotsId(), id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameType", id?.ToString() ?? "null");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the IP address search index page
        /// </summary>
        /// <returns>The IP search index view</returns>
        [HttpGet]
        public IActionResult IpIndex()
        {
            try
            {
                logger.LogInformation("User {UserId} accessing IP search index", User.XtremeIdiotsId());
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading IP search index for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Returns players data as JSON for DataTable with optional game type filter
        /// </summary>
        /// <param name="id">Optional game type to filter players by</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data formatted for DataTable consumption</returns>
        [HttpPost]
        public async Task<IActionResult> GetPlayersAjax(GameType? id, CancellationToken cancellationToken = default)
        {
            return await GetPlayersAjaxPrivate(PlayersFilter.UsernameAndGuid, id, cancellationToken);
        }

        /// <summary>
        /// Returns players data as JSON for IP address search DataTable
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data formatted for DataTable consumption</returns>
        [HttpPost]
        public async Task<IActionResult> GetIpSearchListAjax(CancellationToken cancellationToken = default)
        {
            return await GetPlayersAjaxPrivate(PlayersFilter.IpAddress, null, cancellationToken);
        }

        /// <summary>
        /// Private method to handle DataTable AJAX requests for player data
        /// </summary>
        /// <param name="filter">The filter type to apply to the search</param>
        /// <param name="gameType">Optional game type filter</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data formatted for DataTable consumption</returns>
        [HttpPost]
        private async Task<IActionResult> GetPlayersAjaxPrivate(PlayersFilter filter, GameType? gameType, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} requesting players data with filter {Filter} and game type {GameType}",
                    User.XtremeIdiotsId(), filter, gameType);

                var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

                if (model == null)
                {
                    logger.LogWarning("Invalid DataTable model received from user {UserId}", User.XtremeIdiotsId());
                    return BadRequest();
                }

                var order = PlayersOrder.LastSeenDesc;
                if (model.Order != null && model.Order.Any())
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

                var playerCollectionApiResponse = await repositoryApiClient.Players.V1.GetPlayers(
                    gameType, filter, model.Search?.Value, model.Start, model.Length, order, PlayerEntityOptions.None);

                if (!playerCollectionApiResponse.IsSuccess || playerCollectionApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve players data for user {UserId} with filter {Filter}",
                        User.XtremeIdiotsId(), filter);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                // Enrich player data with ProxyCheck information
                var enrichedPlayers = await playerCollectionApiResponse.Result.Data.Items.EnrichWithProxyCheckDataAsync(proxyCheckService, logger);

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

                logger.LogInformation("Successfully retrieved {Count} players for user {UserId} with filter {Filter}",
                    playerData.Count, User.XtremeIdiotsId(), filter);

                return Json(new
                {
                    model.Draw,
                    recordsTotal = playerCollectionApiResponse.Result.Data.TotalCount,
                    recordsFiltered = playerCollectionApiResponse.Result.Data.FilteredCount,
                    data = playerData
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving players data for user {UserId} with filter {Filter} and game type {GameType}",
                    User.XtremeIdiotsId(), filter, gameType);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("Filter", filter.ToString());
                errorTelemetry.Properties.TryAdd("GameType", gameType?.ToString() ?? "null");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }
        /// <summary>
        /// Displays detailed information for a specific player including aliases, IP addresses, admin actions, and related data
        /// </summary>
        /// <param name="id">The player ID to display details for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The player details view with enriched data or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing player details for player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                var playerApiResponse = await repositoryApiClient.Players.V1.GetPlayer(id,
                    PlayerEntityOptions.Aliases | PlayerEntityOptions.IpAddresses | PlayerEntityOptions.AdminActions |
                    PlayerEntityOptions.RelatedPlayers | PlayerEntityOptions.ProtectedNames | PlayerEntityOptions.Tags);

                if (playerApiResponse.IsNotFound)
                {
                    logger.LogWarning("Player {PlayerId} not found when accessing details", id);
                    return NotFound();
                }

                if (playerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Player data is null for {PlayerId}", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var playerData = playerApiResponse.Result.Data;

                // Check game-specific authorization for viewing player details
                var canViewPlayer = await authorizationService.AuthorizeAsync(User, playerData.GameType, AuthPolicies.ViewPlayers);
                if (!canViewPlayer.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to view player {PlayerId} details for game {GameType}",
                        User.XtremeIdiotsId(), id, playerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(playerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Players");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Details");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "Player");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var playerDetailsViewModel = new PlayerDetailsViewModel
                {
                    Player = playerData
                };

                // Enrich the current player IP with geolocation data (legacy behavior)
                if (!string.IsNullOrWhiteSpace(playerData.IpAddress))
                {
                    try
                    {
                        var getGeoLocationResult = await geoLocationClient.GeoLookup.V1.GetGeoLocation(playerData.IpAddress);

                        if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result?.Data != null)
                            playerDetailsViewModel.GeoLocation = getGeoLocationResult.Result.Data;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to retrieve geolocation for IP {IpAddress} for player {PlayerId}",
                            playerData.IpAddress, id);
                        telemetryClient.TrackException(ex);
                    }
                }

                // Enrich all IP addresses with geolocation and proxy check data
                if (playerData.PlayerIpAddresses != null && playerData.PlayerIpAddresses.Any())
                {
                    foreach (var ipAddress in playerData.PlayerIpAddresses)
                    {
                        var enrichedIp = new PlayerIpAddressViewModel
                        {
                            IpAddressDto = ipAddress,
                            IsCurrentIp = ipAddress.Address == playerData.IpAddress
                        };

                        try
                        {
                            // Get geolocation data
                            var getGeoLocationResult = await geoLocationClient.GeoLookup.V1.GetGeoLocation(ipAddress.Address);
                            if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result != null)
                            {
                                enrichedIp.GeoLocation = getGeoLocationResult.Result.Data;
                            }

                            // Get proxy check data
                            var proxyCheck = await proxyCheckService.GetIpRiskDataAsync(ipAddress.Address);
                            if (!proxyCheck.IsError)
                            {
                                enrichedIp.ProxyCheck = proxyCheck;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to enrich IP address {IpAddress} for player {PlayerId}",
                                ipAddress.Address, id);
                            telemetryClient.TrackException(ex);
                        }

                        playerDetailsViewModel.EnrichedIpAddresses.Add(enrichedIp);
                    }
                }

                logger.LogInformation("Successfully loaded player details for user {UserId} and player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                return View(playerDetailsViewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading player details for player {PlayerId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays admin actions created by the current user
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The user's admin actions view</returns>
        [HttpGet]
        public async Task<IActionResult> MyActions(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing their admin actions", User.XtremeIdiotsId());

                var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, User.XtremeIdiotsId(), null, 0, 50, AdminActionOrder.CreatedDesc);

                if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve admin actions for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully retrieved {Count} admin actions for user {UserId}",
                    adminActionsApiResponse.Result.Data.Items.Count(), User.XtremeIdiotsId());

                return View(adminActionsApiResponse.Result.Data.Items);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving admin actions for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays unclaimed ban admin actions that can be claimed by moderators
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The unclaimed admin actions view</returns>
        [HttpGet]
        public async Task<IActionResult> Unclaimed(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing unclaimed admin actions", User.XtremeIdiotsId());

                var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, null, AdminActionFilter.UnclaimedBans, 0, 50, AdminActionOrder.CreatedDesc);

                if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve unclaimed admin actions for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully retrieved {Count} unclaimed admin actions for user {UserId}",
                    adminActionsApiResponse.Result.Data.Items.Count(), User.XtremeIdiotsId());

                return View(adminActionsApiResponse.Result.Data.Items);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving unclaimed admin actions for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the player analytics page
        /// </summary>
        /// <returns>The analytics view with date filter range</returns>
        [HttpGet]
        public IActionResult Analytics()
        {
            try
            {
                logger.LogInformation("User {UserId} accessing player analytics", User.XtremeIdiotsId());

                var cutoff = DateTime.UtcNow.AddMonths(-3);
                ViewBag.DateFilterRange = cutoff;

                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading player analytics for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Returns cumulative daily players data as JSON for analytics charts
        /// </summary>
        /// <param name="cutoff">The cutoff date to filter data from</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data for cumulative daily players chart</returns>
        [HttpGet]
        public async Task<IActionResult> GetCumulativeDailyPlayersJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} requesting cumulative daily players data from {Cutoff}",
                    User.XtremeIdiotsId(), cutoff);

                var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetCumulativeDailyPlayers(cutoff);

                if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve cumulative daily players data for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully retrieved cumulative daily players data for user {UserId}", User.XtremeIdiotsId());

                return Json(playerAnalyticsResponse.Result.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving cumulative daily players data for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("Cutoff", cutoff.ToString("yyyy-MM-dd"));
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Returns new daily players per game data as JSON for analytics charts
        /// </summary>
        /// <param name="cutoff">The cutoff date to filter data from</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data for new daily players per game chart</returns>
        [HttpGet]
        public async Task<IActionResult> GetNewDailyPlayersPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} requesting new daily players per game data from {Cutoff}",
                    User.XtremeIdiotsId(), cutoff);

                var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetNewDailyPlayersPerGame(cutoff);

                if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve new daily players per game data for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully retrieved new daily players per game data for user {UserId}", User.XtremeIdiotsId());

                return Json(playerAnalyticsResponse.Result.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving new daily players per game data for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("Cutoff", cutoff.ToString("yyyy-MM-dd"));
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Returns players drop-off per game data as JSON for analytics charts
        /// </summary>
        /// <param name="cutoff">The cutoff date to filter data from</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data for players drop-off per game chart</returns>
        [HttpGet]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} requesting players drop-off per game data from {Cutoff}",
                    User.XtremeIdiotsId(), cutoff);

                var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetPlayersDropOffPerGameJson(cutoff);

                if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve players drop-off per game data for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully retrieved players drop-off per game data for user {UserId}", User.XtremeIdiotsId());

                return Json(playerAnalyticsResponse.Result.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving players drop-off per game data for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("Cutoff", cutoff.ToString("yyyy-MM-dd"));
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        #region Protected Names

        /// <summary>
        /// Displays the list of all protected player names
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The protected names view with the list of protected names</returns>
        [HttpGet]
        public async Task<IActionResult> ProtectedNames(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing protected names list", User.XtremeIdiotsId());

                // Check authorization for viewing protected names
                var canViewProtectedNames = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.ViewProtectedName);
                if (!canViewProtectedNames.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to view protected names",
                        User.XtremeIdiotsId());

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Players");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "ProtectedNames");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Context", "ViewAll");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var protectedNamesResponse = await repositoryApiClient.Players.V1.GetProtectedNames(0, 1000);

                if (!protectedNamesResponse.IsSuccess || protectedNamesResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve protected names for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new ProtectedNamesViewModel
                {
                    ProtectedNames = protectedNamesResponse.Result.Data.Items.ToList()
                };

                logger.LogInformation("Successfully retrieved {Count} protected names for user {UserId}",
                    model.ProtectedNames.Count, User.XtremeIdiotsId());

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving protected names for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the form to add a protected name for a specific player
        /// </summary>
        /// <param name="id">The player ID to add a protected name for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The add protected name view or error response</returns>
        [HttpGet]
        public async Task<IActionResult> AddProtectedName(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing add protected name form for player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                var canCreateProtectedName = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.CreateProtectedName);
                if (!canCreateProtectedName.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create protected name for player {PlayerId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Players");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "AddProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

                if (playerResponse.IsNotFound)
                {
                    logger.LogWarning("Player {PlayerId} not found when adding protected name", id);
                    return NotFound();
                }

                if (!playerResponse.IsSuccess || playerResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve player {PlayerId} for protected name", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new CreateProtectedNameViewModel(id)
                {
                    Player = playerResponse.Result.Data
                };

                logger.LogInformation("Successfully loaded add protected name form for user {UserId} and player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading add protected name form for player {PlayerId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Creates a new protected name for a player based on the submitted form data
        /// </summary>
        /// <param name="model">The create protected name view model containing the protection details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to player details page on success, or returns the view with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProtectedName(CreateProtectedNameViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to create protected name for player {PlayerId}",
                    User.XtremeIdiotsId(), model.PlayerId);

                // Load player data for authorization and telemetry enrichment
                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                if (playerResponse.IsNotFound)
                {
                    logger.LogWarning("Player {PlayerId} not found when creating protected name", model.PlayerId);
                    return NotFound();
                }

                if (!playerResponse.IsSuccess || playerResponse.Result?.Data == null)
                {
                    logger.LogWarning("Player data is null for {PlayerId} when creating protected name", model.PlayerId);
                    return BadRequest();
                }

                var playerData = playerResponse.Result.Data;

                var canCreateProtectedName = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.CreateProtectedName);
                if (!canCreateProtectedName.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create protected name for player {PlayerId}",
                        User.XtremeIdiotsId(), model.PlayerId);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(playerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Players");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "AddProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{model.PlayerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for creating protected name for player {PlayerId}", model.PlayerId);
                    model.Player = playerData;
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
                        logger.LogWarning("Protected name '{ProtectedName}' already exists for another player when user {UserId} attempted to protect it for player {PlayerId}",
                            model.Name, User.XtremeIdiotsId(), model.PlayerId);

                        ModelState.AddModelError("Name", "This name is already protected by another player");
                        model.Player = playerData;
                        return View(model);
                    }

                    logger.LogWarning("Failed to create protected name for player {PlayerId} by user {UserId}",
                        model.PlayerId, User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully created protected name '{ProtectedName}' for player {PlayerId} by user {UserId}",
                    model.Name, model.PlayerId, User.XtremeIdiotsId());

                var eventTelemetry = new EventTelemetry("ProtectedNameCreated")
                    .Enrich(User)
                    .Enrich(playerData);
                eventTelemetry.Properties.TryAdd("ProtectedName", model.Name);
                telemetryClient.TrackEvent(eventTelemetry);

                this.AddAlertSuccess($"Protected name '{model.Name}' has been successfully added");

                return RedirectToAction(nameof(Details), new { id = model.PlayerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating protected name for player {PlayerId}", model.PlayerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", model.PlayerId.ToString());
                errorTelemetry.Properties.TryAdd("ProtectedName", model.Name ?? "null");
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while adding the protected name");
                return RedirectToAction(nameof(Details), new { id = model.PlayerId });
            }
        }

        /// <summary>
        /// Deletes a protected name by ID
        /// </summary>
        /// <param name="id">The protected name ID to delete</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to player details page on success, or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> DeleteProtectedName(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to delete protected name {ProtectedNameId}",
                    User.XtremeIdiotsId(), id);

                var canDeleteProtectedName = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.DeleteProtectedName);
                if (!canDeleteProtectedName.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete protected name {ProtectedNameId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Players");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "DeleteProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ProtectedNameId:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var protectedNameResponse = await repositoryApiClient.Players.V1.GetProtectedName(id);

                if (protectedNameResponse.IsNotFound)
                {
                    logger.LogWarning("Protected name {ProtectedNameId} not found when deleting", id);
                    return NotFound();
                }

                if (!protectedNameResponse.IsSuccess || protectedNameResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve protected name {ProtectedNameId} for deletion", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var playerId = protectedNameResponse.Result.Data.PlayerId;
                var deleteProtectedNameDto = new DeleteProtectedNameDto(id);
                var response = await repositoryApiClient.Players.V1.DeleteProtectedName(deleteProtectedNameDto);

                if (!response.IsSuccess)
                {
                    logger.LogWarning("Failed to delete protected name {ProtectedNameId} for user {UserId}",
                        id, User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully deleted protected name {ProtectedNameId} by user {UserId}",
                    id, User.XtremeIdiotsId());

                var eventTelemetry = new EventTelemetry("ProtectedNameDeleted")
                    .Enrich(User);
                eventTelemetry.Properties.TryAdd("ProtectedNameId", id.ToString());
                eventTelemetry.Properties.TryAdd("PlayerId", playerId.ToString());
                telemetryClient.TrackEvent(eventTelemetry);

                this.AddAlertSuccess("Protected name has been successfully deleted");

                return RedirectToAction(nameof(Details), new { id = playerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting protected name {ProtectedNameId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ProtectedNameId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the usage report for a specific protected name
        /// </summary>
        /// <param name="id">The protected name ID to generate report for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The protected name report view or error response</returns>
        [HttpGet]
        public async Task<IActionResult> ProtectedNameReport(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing protected name report for {ProtectedNameId}",
                    User.XtremeIdiotsId(), id);

                var reportResponse = await repositoryApiClient.Players.V1.GetProtectedNameUsageReport(id);

                if (reportResponse.IsNotFound)
                {
                    logger.LogWarning("Protected name report {ProtectedNameId} not found", id);
                    return NotFound();
                }

                if (!reportResponse.IsSuccess || reportResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve protected name report {ProtectedNameId}", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new ProtectedNameReportViewModel
                {
                    Report = reportResponse.Result.Data
                };

                logger.LogInformation("Successfully retrieved protected name report {ProtectedNameId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving protected name report {ProtectedNameId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ProtectedNameId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        #endregion        #region Player Tags

        /// <summary>
        /// Displays the form to add a tag to a specific player
        /// </summary>
        /// <param name="id">The player ID to add a tag to</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The add player tag view or error response</returns>
        [HttpGet]
        public async Task<IActionResult> AddPlayerTag(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing add player tag form for player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                var canCreatePlayerTag = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.CreatePlayerTag);
                if (!canCreatePlayerTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create player tag for player {PlayerId}",
                        User.XtremeIdiotsId(), id);

                    telemetryClient.TrackEvent("UnauthorizedUserAccessAttempt",
                        new Dictionary<string, string>
                        {
                            { "Area", "PlayersController" },
                            { "Action", "AddPlayerTag" },
                            { "UserId", User.XtremeIdiotsId() },
                            { "UserDisplayName", User.Identity?.Name ?? "Unknown" },
                            { "PlayerGuid", id.ToString() },
                            { "Reason", "InsufficientPermissions" }
                        });

                    return Unauthorized();
                }

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

                if (playerResponse.IsNotFound)
                {
                    logger.LogWarning("Player {PlayerId} not found when adding player tag", id);
                    return NotFound();
                }

                if (!playerResponse.IsSuccess || playerResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve player {PlayerId} for player tag", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);

                if (!tagsResponse.IsSuccess || tagsResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve tags for player tag assignment");
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new AddPlayerTagViewModel
                {
                    PlayerId = id,
                    Player = playerResponse.Result.Data,
                    AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList()
                };

                logger.LogInformation("Successfully loaded add player tag form for user {UserId} and player {PlayerId} with {TagCount} available tags",
                    User.XtremeIdiotsId(), id, model.AvailableTags.Count);

                telemetryClient.TrackEvent("PlayerTagAddPageViewed",
                    new Dictionary<string, string>
                    {
                        { "PlayerId", id.ToString() },
                        { "PlayerUsername", playerResponse.Result.Data.Username },
                        { "GameType", playerResponse.Result.Data.GameType.ToString() },
                        { "UserId", User.XtremeIdiotsId() },
                        { "UserDisplayName", User.Identity?.Name ?? "Unknown" },
                        { "AvailableTagCount", model.AvailableTags.Count.ToString() }
                    });

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading add player tag form for player {PlayerId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Creates a new player tag assignment based on the submitted form data
        /// </summary>
        /// <param name="model">The add player tag view model containing the tag assignment details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to player details page on success, or returns the view with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPlayerTag(AddPlayerTagViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to add tag {TagId} to player {PlayerId}",
                    User.XtremeIdiotsId(), model.TagId, model.PlayerId);

                var canCreatePlayerTag = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.CreatePlayerTag);
                if (!canCreatePlayerTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to add tag {TagId} to player {PlayerId}",
                        User.XtremeIdiotsId(), model.TagId, model.PlayerId);

                    telemetryClient.TrackEvent("UnauthorizedUserAccessAttempt",
                        new Dictionary<string, string>
                        {
                            { "Area", "PlayersController" },
                            { "Action", "AddPlayerTag" },
                            { "UserId", User.XtremeIdiotsId() },
                            { "UserDisplayName", User.Identity?.Name ?? "Unknown" },
                            { "PlayerGuid", model.PlayerId.ToString() },
                            { "TagId", model.TagId.ToString() },
                            { "Reason", "InsufficientPermissions" }
                        });

                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for adding tag to player {PlayerId}", model.PlayerId);

                    var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                    if (playerResponse.IsSuccess && playerResponse.Result?.Data != null)
                    {
                        model.Player = playerResponse.Result.Data;
                    }
                    var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);
                    if (tagsResponse.IsSuccess && tagsResponse.Result?.Data?.Items != null)
                    {
                        model.AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList();
                    }
                    return View(model);
                }

                var tagResponse = await repositoryApiClient.Tags.V1.GetTag(model.TagId);
                if (!tagResponse.IsSuccess || tagResponse.Result?.Data == null)
                {
                    logger.LogWarning("Tag {TagId} not found when adding to player {PlayerId}", model.TagId, model.PlayerId);
                    return RedirectToAction("Display", "Errors", new { id = 404 });
                }

                // Check if this tag is UserDefined - only those can be added by users
                if (!tagResponse.Result.Data.UserDefined)
                {
                    logger.LogWarning("User {UserId} attempted to assign non-user-defined tag {TagId} to player {PlayerId}",
                        User.XtremeIdiotsId(), model.TagId, model.PlayerId);

                    this.AddAlertDanger("This tag cannot be assigned to players as it is not marked as User Defined.");

                    var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                    if (playerResponse.IsSuccess && playerResponse.Result?.Data != null)
                    {
                        model.Player = playerResponse.Result.Data;
                    }

                    var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);
                    if (tagsResponse.IsSuccess && tagsResponse.Result?.Data?.Items != null)
                    {
                        model.AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList();
                    }

                    return View(model);
                }

                var userProfileIdString = User.UserProfileId();
                if (string.IsNullOrWhiteSpace(userProfileIdString) || !Guid.TryParse(userProfileIdString, out var userProfileId))
                {
                    logger.LogWarning("Invalid user profile ID for user {UserId} when adding player tag", User.XtremeIdiotsId());
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
                {
                    logger.LogWarning("Failed to add tag {TagId} to player {PlayerId} for user {UserId}",
                        model.TagId, model.PlayerId, User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully added tag '{TagName}' ({TagId}) to player {PlayerId} by user {UserId}",
                    tagResponse.Result.Data.Name, model.TagId, model.PlayerId, User.XtremeIdiotsId());

                // Get player data for telemetry enrichment
                var playerDataResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                var playerData = playerDataResponse.IsSuccess ? playerDataResponse.Result?.Data : null;

                telemetryClient.TrackEvent("PlayerTagAdded",
                    new Dictionary<string, string>
                    {
                        { "PlayerId", model.PlayerId.ToString() },
                        { "PlayerUsername", playerData?.Username ?? "Unknown" },
                        { "GameType", playerData?.GameType.ToString() ?? "Unknown" },
                        { "TagId", model.TagId.ToString() },
                        { "TagName", tagResponse.Result.Data.Name },
                        { "UserId", User.XtremeIdiotsId() },
                        { "UserDisplayName", User.Identity?.Name ?? "Unknown" }
                    });

                this.AddAlertSuccess($"The tag '{tagResponse.Result.Data.Name}' has been successfully added to the player");

                return RedirectToAction(nameof(Details), new { id = model.PlayerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding tag {TagId} to player {PlayerId}", model.TagId, model.PlayerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", model.PlayerId.ToString());
                errorTelemetry.Properties.TryAdd("TagId", model.TagId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while adding the tag to the player");
                return RedirectToAction(nameof(Details), new { id = model.PlayerId });
            }
        }
        /// <summary>
        /// Displays the confirmation page for removing a player tag
        /// </summary>
        /// <param name="id">The player ID to remove the tag from</param>
        /// <param name="playerTagId">The player tag ID to remove</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The remove player tag confirmation view or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> RemovePlayerTag(Guid id, Guid playerTagId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing remove player tag confirmation for player {PlayerId} and tag {PlayerTagId}",
                    User.XtremeIdiotsId(), id, playerTagId);

                var canDeletePlayerTag = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.DeletePlayerTag);
                if (!canDeletePlayerTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to remove player tag {PlayerTagId} from player {PlayerId}",
                        User.XtremeIdiotsId(), playerTagId, id);

                    telemetryClient.TrackEvent("UnauthorizedUserAccessAttempt",
                        new Dictionary<string, string>
                        {
                            { "Area", "PlayersController" },
                            { "Action", "RemovePlayerTag" },
                            { "UserId", User.XtremeIdiotsId() },
                            { "UserDisplayName", User.Identity?.Name ?? "Unknown" },
                            { "PlayerGuid", id.ToString() },
                            { "PlayerTagId", playerTagId.ToString() },
                            { "Reason", "InsufficientPermissions" }
                        });

                    return Unauthorized();
                }

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);
                if (playerResponse.IsNotFound || playerResponse.Result?.Data == null)
                {
                    logger.LogWarning("Player {PlayerId} not found when removing player tag {PlayerTagId}", id, playerTagId);
                    return NotFound();
                }

                var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(id);
                if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve player tags for player {PlayerId}", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var playerTag = playerTagsResponse.Result.Data.Items.FirstOrDefault(pt => pt.PlayerTagId == playerTagId);
                if (playerTag == null)
                {
                    logger.LogWarning("Player tag {PlayerTagId} not found for player {PlayerId}", playerTagId, id);
                    return RedirectToAction("Display", "Errors", new { id = 404 });
                }

                // Check if the tag is UserDefined - only those can be removed
                if (playerTag.Tag != null && !playerTag.Tag.UserDefined)
                {
                    logger.LogWarning("User {UserId} attempted to remove non-user-defined player tag {PlayerTagId} from player {PlayerId}",
                        User.XtremeIdiotsId(), playerTagId, id);

                    this.AddAlertDanger("This tag cannot be removed as it is not marked as User Defined.");
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                ViewBag.Player = playerResponse.Result.Data;

                logger.LogInformation("Successfully loaded remove player tag confirmation for user {UserId}, player {PlayerId}, and tag {PlayerTagId}",
                    User.XtremeIdiotsId(), id, playerTagId);

                return View(playerTag);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading remove player tag confirmation for player {PlayerId} and tag {PlayerTagId}",
                    id, playerTagId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", id.ToString());
                errorTelemetry.Properties.TryAdd("PlayerTagId", playerTagId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }
        /// <summary>
        /// Confirms the removal of a player tag assignment
        /// </summary>
        /// <param name="id">The player ID to remove the tag from</param>
        /// <param name="playerTagId">The player tag ID to remove</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to player details page on success, or appropriate error response</returns>
        [HttpPost]
        [ActionName("RemovePlayerTag")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePlayerTagConfirmed(Guid id, Guid playerTagId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to remove player tag {PlayerTagId} from player {PlayerId}",
                    User.XtremeIdiotsId(), playerTagId, id);

                var canDeletePlayerTag = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.DeletePlayerTag);
                if (!canDeletePlayerTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to remove player tag {PlayerTagId} from player {PlayerId}",
                        User.XtremeIdiotsId(), playerTagId, id);

                    telemetryClient.TrackEvent("UnauthorizedUserAccessAttempt",
                        new Dictionary<string, string>
                        {
                            { "Area", "PlayersController" },
                            { "Action", "RemovePlayerTag" },
                            { "UserId", User.XtremeIdiotsId() },
                            { "UserDisplayName", User.Identity?.Name ?? "Unknown" },
                            { "PlayerGuid", id.ToString() },
                            { "PlayerTagId", playerTagId.ToString() },
                            { "Reason", "InsufficientPermissions" }
                        });

                    return Unauthorized();
                }

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);
                if (playerResponse.IsNotFound || playerResponse.Result?.Data == null)
                {
                    logger.LogWarning("Player {PlayerId} not found when removing player tag {PlayerTagId}", id, playerTagId);
                    return NotFound();
                }

                var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(id);
                if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve player tags for player {PlayerId}", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var playerTag = playerTagsResponse.Result.Data.Items.FirstOrDefault(pt => pt.PlayerTagId == playerTagId);
                if (playerTag == null)
                {
                    logger.LogWarning("Player tag {PlayerTagId} not found for player {PlayerId}", playerTagId, id);
                    return RedirectToAction("Display", "Errors", new { id = 404 });
                }

                // Check if the tag is UserDefined - only those can be removed
                if (playerTag.Tag != null && !playerTag.Tag.UserDefined)
                {
                    logger.LogWarning("User {UserId} attempted to remove non-user-defined player tag {PlayerTagId} from player {PlayerId}",
                        User.XtremeIdiotsId(), playerTagId, id);

                    this.AddAlertDanger("This tag cannot be removed as it is not marked as User Defined.");
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                var response = await repositoryApiClient.Players.V1.RemovePlayerTag(id, playerTagId);

                if (!response.IsSuccess)
                {
                    logger.LogWarning("Failed to remove player tag {PlayerTagId} from player {PlayerId} for user {UserId}",
                        playerTagId, id, User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully removed player tag '{TagName}' ({PlayerTagId}) from player {PlayerId} by user {UserId}",
                    playerTag.Tag?.Name ?? "Unknown", playerTagId, id, User.XtremeIdiotsId());

                telemetryClient.TrackEvent("PlayerTagRemoved",
                    new Dictionary<string, string>
                    {
                        { "PlayerId", id.ToString() },
                        { "PlayerUsername", playerResponse.Result?.Data?.Username ?? "Unknown" },
                        { "GameType", playerResponse.Result?.Data?.GameType.ToString() ?? "Unknown" },
                        { "PlayerTagId", playerTagId.ToString() },
                        { "TagName", playerTag.Tag?.Name ?? "Unknown" },
                        { "UserId", User.XtremeIdiotsId() },
                        { "UserDisplayName", User.Identity?.Name ?? "Unknown" }
                    });

                this.AddAlertSuccess($"The tag '{playerTag.Tag?.Name ?? "Unknown"}' has been successfully removed from the player");

                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing player tag {PlayerTagId} from player {PlayerId}", playerTagId, id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", id.ToString());
                errorTelemetry.Properties.TryAdd("PlayerTagId", playerTagId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while removing the tag from the player");
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }
    }
}