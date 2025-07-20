using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MX.GeoLocation.Api.Client.V1;
using Newtonsoft.Json;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing core player operations including search, details, and IP addresses
/// </summary>
[Authorize(Policy = AuthPolicies.AccessPlayers)]
public class PlayersController : BaseController
{
    private readonly IAuthorizationService authorizationService;
    private readonly IGeoLocationApiClient geoLocationClient;
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly IProxyCheckService proxyCheckService;

    /// <summary>
    /// Initializes a new instance of the PlayersController
    /// </summary>
    /// <param name="authorizationService">Service for handling authorization policies and requirements</param>
    /// <param name="geoLocationClient">Client for geolocation API services to enrich IP address data</param>
    /// <param name="repositoryApiClient">Client for repository API to access player and related data</param>
    /// <param name="telemetryClient">Application Insights telemetry client for tracking and monitoring</param>
    /// <param name="proxyCheckService">Service for checking IP addresses against proxy/VPN databases</param>
    /// <param name="logger">Logger instance for structured logging and diagnostics</param>
    /// <param name="configuration">Application configuration for settings and connection strings</param>
    /// <exception cref="ArgumentNullException">Thrown when any required service is null</exception>
    public PlayersController(
    IAuthorizationService authorizationService,
    IGeoLocationApiClient geoLocationClient,
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    IProxyCheckService proxyCheckService,
    ILogger<PlayersController> logger,
    IConfiguration configuration) : base(telemetryClient, logger, configuration)
    {
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        this.proxyCheckService = proxyCheckService ?? throw new ArgumentNullException(nameof(proxyCheckService));
    }

    /// <summary>
    /// Displays the main players index page with search capabilities
    /// </summary>
    /// <returns>The players index view for browsing and searching players</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return await ExecuteWithErrorHandlingAsync(() =>
        {
            return Task.FromResult(View() as IActionResult);
        }, nameof(Index));
    }

    /// <summary>
    /// Displays the players index page filtered by game type
    /// </summary>
    /// <param name="id">The game type to filter players by</param>
    /// <returns>The players index view with game type filter applied</returns>
    [HttpGet]
    public async Task<IActionResult> GameIndex(GameType? id)
    {
        return await ExecuteWithErrorHandlingAsync(() =>
        {
            ViewData["GameType"] = id;
            return Task.FromResult(View(nameof(Index)) as IActionResult);
        }, nameof(GameIndex));
    }

    /// <summary>
    /// Displays the IP address search index page
    /// </summary>
    /// <returns>The IP search index view for IP-based player searches</returns>
    [HttpGet]
    public async Task<IActionResult> IpIndex()
    {
        return await ExecuteWithErrorHandlingAsync(() =>
        {
            return Task.FromResult(View() as IActionResult);
        }, nameof(IpIndex));
    }

    /// <summary>
    /// Returns players data as JSON for DataTable with optional game type filter
    /// </summary>
    /// <param name="id">Optional game type to filter players by</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>JSON data formatted for DataTable consumption with player information</returns>
    [HttpPost]
    public async Task<IActionResult> GetPlayersAjax(GameType? id, CancellationToken cancellationToken = default)
    {
        return await GetPlayersAjaxPrivate(PlayersFilter.UsernameAndGuid, id, cancellationToken);
    }

    /// <summary>
    /// Returns players data as JSON for IP address search DataTable
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>JSON data formatted for DataTable consumption with IP address search results</returns>
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
    /// <exception cref="ArgumentException">Thrown when invalid DataTable model is received</exception>
    /// <exception cref="HttpRequestException">Thrown when repository API call fails</exception>
    private async Task<IActionResult> GetPlayersAjaxPrivate(PlayersFilter filter, GameType? gameType, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model is null)
            {
                Logger.LogWarning("Invalid DataTable model received from user {UserId}", User.XtremeIdiotsId());
                return BadRequest();
            }

            var order = GetPlayersOrderFromDataTable(model);

            var playerCollectionApiResponse = await repositoryApiClient.Players.V1.GetPlayers(
     gameType, filter, model.Search?.Value, model.Start, model.Length, order, PlayerEntityOptions.None);

            if (!playerCollectionApiResponse.IsSuccess || playerCollectionApiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve players data for user {UserId} with filter {Filter}",
         User.XtremeIdiotsId(), filter);
                return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController)[..^10], new { id = 500 });
            }

            // Enrich player data with ProxyCheck information
            var enrichedPlayers = await playerCollectionApiResponse.Result.Data.Items.EnrichWithProxyCheckDataAsync(proxyCheckService, Logger);

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
                ProxyCheckRiskScore = player.ProxyCheckRiskScore(),
                IsProxy = player.IsProxy(),
                IsVpn = player.IsVpn()
            }).ToList();

            Logger.LogInformation("Successfully retrieved {Count} players for user {UserId} with filter {Filter}",
     playerData.Count, User.XtremeIdiotsId(), filter);

            return Json(new
            {
                model.Draw,
                recordsTotal = playerCollectionApiResponse.Result.Data.TotalCount,
                recordsFiltered = playerCollectionApiResponse.Result.Data.FilteredCount,
                data = playerData
            });
        }, nameof(GetPlayersAjaxPrivate));
    }

    /// <summary>
    /// Helper method to determine the correct PlayersOrder from DataTable model
    /// </summary>
    /// <param name="model">The DataTable AJAX model containing order information</param>
    /// <returns>The appropriate PlayersOrder enum value based on column and direction</returns>
    private static PlayersOrder GetPlayersOrderFromDataTable(DataTableAjaxPostModel model)
    {
        var order = PlayersOrder.LastSeenDesc;

        if (model.Order is not null && model.Order.Any())
        {
            var orderColumn = model.Columns[model.Order.First().Column].Name;
            var searchOrder = model.Order.First().Dir;

            order = orderColumn switch
            {
                "gameType" => searchOrder == "asc" ? PlayersOrder.GameTypeAsc : PlayersOrder.GameTypeDesc,
                "username" => searchOrder == "asc" ? PlayersOrder.UsernameAsc : PlayersOrder.UsernameDesc,
                "firstSeen" => searchOrder == "asc" ? PlayersOrder.FirstSeenAsc : PlayersOrder.FirstSeenDesc,
                "lastSeen" => searchOrder == "asc" ? PlayersOrder.LastSeenAsc : PlayersOrder.LastSeenDesc,
                _ => PlayersOrder.LastSeenDesc
            };
        }

        return order;
    }

    /// <summary>
    /// Displays detailed information for a specific player including aliases, IP addresses, admin actions and related data
    /// </summary>
    /// <param name="id">The player ID to display details for</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The player details view with enriched data or appropriate error response</returns>
    /// <exception cref="NotFoundException">Thrown when the specified player is not found</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionview player details for the game type</exception>
    /// <exception cref="HttpRequestException">Thrown when repository API or external service calls fail</exception>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var (actionResult, playerData) = await GetAuthorizedPlayerAsync(id, "view", cancellationToken);
            if (actionResult is not null) return actionResult;

            var playerDetailsViewModel = new PlayerDetailsViewModel
            {
                Player = playerData!
            };

            // Enrich the current player IP with geolocation data (legacy behavior)
            await EnrichCurrentPlayerGeoLocationAsync(playerDetailsViewModel, playerData!, id);

            // Enrich all IP addresses with geolocation and proxy check data
            if (playerData!.PlayerIpAddresses is not null && playerData.PlayerIpAddresses.Any())
            {
                await EnrichPlayerIpAddressesAsync(playerDetailsViewModel, playerData, id);
            }

            TrackSuccessTelemetry("PlayerDetailsViewed", "ViewPlayerDetails", new Dictionary<string, string>
        {
 { "PlayerId", id.ToString() },
 { "GameType", playerData.GameType.ToString() },
 { "IpAddressCount", playerData.PlayerIpAddresses?.Count.ToString() ?? "0" }
        });

            return View(playerDetailsViewModel);
        }, nameof(Details));
    }

    /// <summary>
    /// Helper method to get and authorize a player for viewing
    /// </summary>
    /// <param name="id">The player ID to retrieve and authorize</param>
    /// <param name="action">The action being performed for logging purposes</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Tuple containing ActionResult (if error occurred) and player data (if successful)</returns>
    /// <exception cref="HttpRequestException">Thrown when repository API call fails</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when authorization check fails</exception>
    private async Task<(IActionResult? ActionResult, PlayerDto? Data)> GetAuthorizedPlayerAsync(
    Guid id,
    string action,
    CancellationToken cancellationToken = default)
    {
        var playerApiResponse = await repositoryApiClient.Players.V1.GetPlayer(id,
        PlayerEntityOptions.Aliases | PlayerEntityOptions.IpAddresses | PlayerEntityOptions.AdminActions |
        PlayerEntityOptions.RelatedPlayers | PlayerEntityOptions.ProtectedNames | PlayerEntityOptions.Tags);

        if (playerApiResponse.IsNotFound)
        {
            Logger.LogWarning("Player {PlayerId} not found when attempting to {Action}", id, action);
            return (NotFound(), null);
        }

        if (playerApiResponse.Result?.Data is null)
        {
            Logger.LogWarning("Player data is null for {PlayerId} when attempting to {Action}", id, action);
            return (RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController)[..^10], new { id = 500 }), null);
        }

        var playerData = playerApiResponse.Result.Data;

        var authResult = await CheckAuthorizationAsync(
        authorizationService,
        playerData.GameType,
        AuthPolicies.ViewPlayers,
        action,
        "Player",
        $"GameType:{playerData.GameType}",
        playerData);

        if (authResult is not null) return (authResult, null);

        return (null, playerData);
    }

    /// <summary>
    /// Enriches the player details view model with geolocation data for the current player IP address
    /// </summary>
    /// <param name="viewModel">The player details view model to enrich with geolocation data</param>
    /// <param name="playerData">The player data containing the IP address to lookup</param>
    /// <param name="playerId">The player ID for logging and error tracking purposes</param>
    /// <remarks>
    /// This method provides legacy geolocation behavior for the main player IP address.
    /// Failures are logged but do not prevent the view from being displayed.
    /// </remarks>
    private async Task EnrichCurrentPlayerGeoLocationAsync(PlayerDetailsViewModel viewModel, PlayerDto playerData, Guid playerId)
    {
        if (string.IsNullOrWhiteSpace(playerData.IpAddress)) return;

        try
        {
            var getGeoLocationResult = await geoLocationClient.GeoLookup.V1.GetGeoLocation(playerData.IpAddress);

            if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result?.Data is not null)
                viewModel.GeoLocation = getGeoLocationResult.Result.Data;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to retrieve geolocation for IP {IpAddress} for player {PlayerId}",
            playerData.IpAddress, playerId);
        }
    }

    /// <summary>
    /// Enriches all player IP addresses with comprehensive geolocation and proxy check data
    /// </summary>
    /// <param name="viewModel">The player details view model to enrich with IP address data</param>
    /// <param name="playerData">The player data containing IP addresses to enrich</param>
    /// <param name="playerId">The player ID for logging and error tracking purposes</param>
    /// <remarks>
    /// This method processes all IP addresses associated with the player, enriching each with:
    /// - Geolocation data (country, region, city information)
    /// - Proxy/VPN detection data for security analysis
    /// - Current IP address flagging for UI display
    /// Individual IP enrichment failures are logged but do not prevent processing of other IPs.
    /// </remarks>
    private async Task EnrichPlayerIpAddressesAsync(PlayerDetailsViewModel viewModel, PlayerDto playerData, Guid playerId)
    {
        foreach (var ipAddress in playerData.PlayerIpAddresses!)
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
                if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result is not null)
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
                Logger.LogWarning(ex, "Failed to enrich IP address {IpAddress} for player {PlayerId}",
                ipAddress.Address, playerId);
            }

            viewModel.EnrichedIpAddresses.Add(enrichedIp);
        }
    }
}
