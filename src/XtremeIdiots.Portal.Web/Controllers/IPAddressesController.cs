using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using MX.GeoLocation.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing IP address details and analysis
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class IPAddressesController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IGeoLocationApiClient geoLocationClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly IProxyCheckService proxyCheckService;
        private readonly ILogger<IPAddressesController> logger;

        public IPAddressesController(
            IAuthorizationService authorizationService,
            IGeoLocationApiClient geoLocationClient,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            IProxyCheckService proxyCheckService,
            ILogger<IPAddressesController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.proxyCheckService = proxyCheckService ?? throw new ArgumentNullException(nameof(proxyCheckService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays detailed information about an IP address including geolocation, proxy status, and associated players
        /// </summary>
        /// <param name="ipAddress">The IP address to analyze</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The IP address details view with comprehensive information, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to view IP address details</exception>
        /// <exception cref="ArgumentException">Thrown when IP address is invalid</exception>
        [HttpGet]
        public async Task<IActionResult> Details(string ipAddress, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    logger.LogWarning("User {UserId} attempted to view IP address details with null or empty IP address",
                        User.XtremeIdiotsId());
                    return NotFound();
                }

                logger.LogInformation("User {UserId} attempting to view IP address details for {IpAddress}",
                    User.XtremeIdiotsId(), ipAddress);

                // Check authorization for viewing IP address details
                var canViewIpDetails = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.ViewPlayers);
                if (!canViewIpDetails.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to view IP address details for {IpAddress}",
                        User.XtremeIdiotsId(), ipAddress);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "IPAddresses");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Details");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "IPAddress");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"IpAddress:{ipAddress}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var viewModel = new IPAddressDetailsViewModel
                {
                    IpAddress = ipAddress
                };

                // Get GeoLocation information
                var getGeoLocationResult = await geoLocationClient.GeoLookup.V1.GetGeoLocation(ipAddress);
                if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result?.Data != null)
                {
                    viewModel.GeoLocation = getGeoLocationResult.Result.Data;
                    logger.LogInformation("Successfully retrieved geolocation data for IP address {IpAddress}", ipAddress);
                }
                else
                {
                    logger.LogWarning("Failed to retrieve geolocation data for IP address {IpAddress}", ipAddress);
                }

                // Get ProxyCheck information
                var proxyCheckResult = await proxyCheckService.GetIpRiskDataAsync(ipAddress);
                viewModel.ProxyCheck = proxyCheckResult;

                if (proxyCheckResult != null)
                {
                    logger.LogInformation("Successfully retrieved proxy check data for IP address {IpAddress}", ipAddress);
                }
                else
                {
                    logger.LogWarning("Failed to retrieve proxy check data for IP address {IpAddress}", ipAddress);
                }

                // Get players who have used this IP address
                var playersResponse = await repositoryApiClient.Players.V1.GetPlayersWithIpAddress(ipAddress, 0, 100, PlayersOrder.LastSeenDesc, PlayerEntityOptions.None);
                if (playersResponse.IsSuccess && playersResponse.Result?.Data != null)
                {
                    viewModel.Players = playersResponse.Result.Data.Items ?? new List<XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players.PlayerDto>();
                    viewModel.TotalPlayersCount = playersResponse.Result.Data.TotalCount;
                    logger.LogInformation("Successfully retrieved {PlayerCount} players associated with IP address {IpAddress}",
                        viewModel.TotalPlayersCount, ipAddress);
                }
                else
                {
                    logger.LogWarning("Failed to retrieve players with IP address {IpAddress}", ipAddress);
                    viewModel.Players = new List<XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players.PlayerDto>();
                    viewModel.TotalPlayersCount = 0;
                }

                logger.LogInformation("User {UserId} successfully viewed IP address details for {IpAddress}",
                    User.XtremeIdiotsId(), ipAddress);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving IP address details for {IpAddress} by user {UserId}",
                    ipAddress, User.XtremeIdiotsId());

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("UserId", User.XtremeIdiotsId());
                exceptionTelemetry.Properties.TryAdd("IpAddress", ipAddress ?? "null");
                exceptionTelemetry.Properties.TryAdd("Controller", "IPAddresses");
                exceptionTelemetry.Properties.TryAdd("Action", "Details");
                telemetryClient.TrackException(exceptionTelemetry);

                this.AddAlertDanger($"An error occurred while retrieving details for IP address {ipAddress}. Please try again.");
                return RedirectToAction("Display", "Errors", new { id = 500 });
            }
        }
    }
}
