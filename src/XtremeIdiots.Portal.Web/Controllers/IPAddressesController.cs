using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MX.GeoLocation.Api.Client.V1;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Handles IP address details and analytics for the XtremeIdiots Portal
/// </summary>
[Authorize(Policy = AuthPolicies.AccessPlayers)]
public class IPAddressesController : BaseController
{
    private readonly IAuthorizationService authorizationService;
    private readonly IGeoLocationApiClient geoLocationClient;
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly IProxyCheckService proxyCheckService;

    /// <summary>
    /// Initializes a new instance of the IPAddressesController
    /// </summary>
    /// <param name="authorizationService">Service for checking user authorization</param>
    /// <param name="geoLocationClient">Client for retrieving geolocation data</param>
    /// <param name="repositoryApiClient">Client for accessing repository data</param>
    /// <param name="proxyCheckService">Service for checking IP proxy/VPN status</param>
    /// <param name="telemetryClient">Client for tracking telemetry data</param>
    /// <param name="logger">Logger instance for this controller</param>
    /// <param name="configuration">Application configuration</param>
    public IPAddressesController(
        IAuthorizationService authorizationService,
        IGeoLocationApiClient geoLocationClient,
        IRepositoryApiClient repositoryApiClient,
        IProxyCheckService proxyCheckService,
        TelemetryClient telemetryClient,
        ILogger<IPAddressesController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        this.proxyCheckService = proxyCheckService ?? throw new ArgumentNullException(nameof(proxyCheckService));
    }

    /// <summary>
    /// Displays detailed information about an IP address including geolocation, proxy status, and associated players
    /// </summary>
    /// <param name="ipAddress">The IP address to analyze</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with IP address details or NotFound if IP address is invalid</returns>
    [HttpGet]
    public async Task<IActionResult> Details(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                Logger.LogWarning("User {UserId} attempted to view IP address details with null or empty IP address",
                    User.XtremeIdiotsId());
                return NotFound();
            }

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                ipAddress,
                AuthPolicies.ViewPlayers,
                nameof(Details),
                nameof(IPAddressesController),
                $"IpAddress:{ipAddress}",
                ipAddress);

            if (authResult != null) return authResult;

            var viewModel = await BuildIPAddressDetailsViewModelAsync(ipAddress, cancellationToken);

            TrackSuccessTelemetry("IPAddressDetailsViewed", nameof(Details), new Dictionary<string, string>
            {
                { "IpAddress", ipAddress },
                { "PlayersCount", viewModel.TotalPlayersCount.ToString() },
                { "HasGeoLocation", (viewModel.GeoLocation != null).ToString() },
                { "HasProxyCheck", (viewModel.ProxyCheck != null).ToString() }
            });

            return View(viewModel);
        }, "ViewIPAddressDetails");
    }

    private async Task<IPAddressDetailsViewModel> BuildIPAddressDetailsViewModelAsync(string ipAddress, CancellationToken cancellationToken)
    {
        var viewModel = new IPAddressDetailsViewModel
        {
            IpAddress = ipAddress
        };

        try
        {
            var getGeoLocationResult = await geoLocationClient.GeoLookup.V1.GetGeoLocation(ipAddress);
            if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result?.Data is not null)
            {
                viewModel.GeoLocation = getGeoLocationResult.Result.Data;
                Logger.LogDebug("Successfully retrieved geolocation data for IP address {IpAddress}", ipAddress);
            }
            else
            {
                Logger.LogDebug("No geolocation data available for IP address {IpAddress}", ipAddress);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to retrieve geolocation data for IP address {IpAddress}", ipAddress);
        }

        try
        {
            var proxyCheckResult = await proxyCheckService.GetIpRiskDataAsync(ipAddress);
            viewModel.ProxyCheck = proxyCheckResult;

            if (proxyCheckResult is not null)
            {
                Logger.LogDebug("Successfully retrieved proxy check data for IP address {IpAddress}", ipAddress);
            }
            else
            {
                Logger.LogDebug("No proxy check data available for IP address {IpAddress}", ipAddress);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to retrieve proxy check data for IP address {IpAddress}", ipAddress);
        }

        var playersResponse = await repositoryApiClient.Players.V1.GetPlayersWithIpAddress(
            ipAddress, 0, 100, PlayersOrder.LastSeenDesc, PlayerEntityOptions.None);

        if (playersResponse.IsSuccess && playersResponse.Result?.Data is not null)
        {
            viewModel.Players = playersResponse.Result.Data.Items ?? new List<XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players.PlayerDto>();
            viewModel.TotalPlayersCount = playersResponse.Result.Data.TotalCount;
            Logger.LogDebug("Successfully retrieved {PlayerCount} players associated with IP address {IpAddress}",
                viewModel.TotalPlayersCount, ipAddress);
        }
        else
        {
            Logger.LogWarning("Failed to retrieve players for IP address {IpAddress}", ipAddress);
            viewModel.Players = new List<XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players.PlayerDto>();
            viewModel.TotalPlayersCount = 0;
        }

        return viewModel;
    }
}