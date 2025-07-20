using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

[Authorize(Policy = AuthPolicies.AccessPlayers)]
public class PlayersController : BaseController
{
    private readonly IAuthorizationService authorizationService;
    private readonly IGeoLocationApiClient geoLocationClient;
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly IProxyCheckService proxyCheckService;

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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return await ExecuteWithErrorHandlingAsync(() =>
        {
            return Task.FromResult(View() as IActionResult);
        }, nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GameIndex(GameType? id)
    {
        return await ExecuteWithErrorHandlingAsync(() =>
        {
            ViewData["GameType"] = id;
            return Task.FromResult(View(nameof(Index)) as IActionResult);
        }, nameof(GameIndex));
    }

    [HttpGet]
    public async Task<IActionResult> IpIndex()
    {
        return await ExecuteWithErrorHandlingAsync(() =>
        {
            return Task.FromResult(View() as IActionResult);
        }, nameof(IpIndex));
    }

    [HttpPost]
    public async Task<IActionResult> GetPlayersAjax(GameType? id, CancellationToken cancellationToken = default)
    {
        return await GetPlayersAjaxPrivate(PlayersFilter.UsernameAndGuid, id, cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> GetIpSearchListAjax(CancellationToken cancellationToken = default)
    {
        return await GetPlayersAjaxPrivate(PlayersFilter.IpAddress, null, cancellationToken);
    }

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

            var enrichedPlayers = await playerCollectionApiResponse.Result.Data.Items.EnrichWithProxyCheckDataAsync(proxyCheckService, Logger);

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

            await EnrichCurrentPlayerGeoLocationAsync(playerDetailsViewModel, playerData!, id);

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

                var getGeoLocationResult = await geoLocationClient.GeoLookup.V1.GetGeoLocation(ipAddress.Address);
                if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result is not null)
                {
                    enrichedIp.GeoLocation = getGeoLocationResult.Result.Data;
                }

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