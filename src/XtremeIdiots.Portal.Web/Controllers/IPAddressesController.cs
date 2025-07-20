using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MX.GeoLocation.Api.Client.V1;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing IP address details and analysis including geolocation, 
/// proxy detection and associated player information across all supported games
/// </summary>
/// <remarks>
/// This controller provides detailed analysis of IP addresses used by players in the gaming community.
/// It integrates with geolocation services, proxy detection APIs and the repository system to provide
/// comprehensive information about IP addresses and their associated players. The controller is essential
/// for administrative oversight and security monitoring of the gaming community.
/// </remarks>
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
 /// <param name="authorizationService">Service for handling authorization checks</param>
 /// <param name="geoLocationClient">Client for geolocation API services</param>
 /// <param name="repositoryApiClient">Client for repository API services</param>
 /// <param name="proxyCheckService">Service for proxy and risk detection</param>
 /// <param name="telemetryClient">Client for tracking telemetry events</param>
 /// <param name="logger">Logger for structured logging</param>
 /// <param name="configuration">Configuration service for application settings</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
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
 /// Displays detailed information about an IP address including geolocation, proxy status and associated players
 /// </summary>
 /// <param name="ipAddress">The IP address to analyze and retrieve comprehensive information for</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// IP address details view with comprehensive information including geolocation data,
 /// proxy detection results and list of associated players. Returns NotFound for invalid IP addresses
 /// or appropriate error response for authorization failures.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionview IP address details</exception>
 /// <exception cref="ArgumentException">Thrown when IP address format is invalid or malformed</exception>
 /// <remarks>
 /// This action provides a comprehensive analysis view for IP addresses used within the gaming community.
 /// The view includes geolocation information, proxy/VPN detection status, risk assessment data,
 /// and a list of all players who have connected from this IP address. This information is crucial
 /// for administrative oversight, security monitoring and identifying potential suspicious activity.
 /// The method gracefully handles failures from external services and continues processing with
 /// partial data if some services are unavailable.
 /// </remarks>
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

 /// <summary>
 /// Builds the IP address details view model with all enriched data from multiple external services
 /// </summary>
 /// <param name="ipAddress">The IP address to analyze and gather comprehensive data for</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// Complete view model with geolocation information, proxy detection results, risk assessment data,
 /// and list of associated players. The method gracefully handles service failures and returns
 /// partial data when external services are unavailable.
 /// </returns>
 /// <remarks>
 /// This method orchestrates calls to multiple external services to build a comprehensive view of an IP address:
 /// - Geolocation API for geographic information and ISP details
 /// - Proxy detection service for VPN/proxy identification and risk scoring
 /// - Repository API for player association data and usage patterns
 /// 
 /// The method implements resilient patterns by catching and logging exceptions from external services
 /// while continuing to process other data sources. This ensures the view remains functional even when
 /// some services are temporarily unavailable.
 /// </remarks>
 private async Task<IPAddressDetailsViewModel> BuildIPAddressDetailsViewModelAsync(string ipAddress, CancellationToken cancellationToken)
 {
 var viewModel = new IPAddressDetailsViewModel
 {
 IpAddress = ipAddress
 };

 // Get GeoLocation information with resilient error handling
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
 // Continue processing without geolocation data
 }

 // Get ProxyCheck information with resilient error handling
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
 // Continue processing without proxy check data
 }

 // Get players who have used this IP address
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
