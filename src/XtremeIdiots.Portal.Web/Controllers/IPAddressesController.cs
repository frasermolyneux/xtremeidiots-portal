using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using MX.GeoLocation.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class IPAddressesController : Controller
    {
        private readonly IGeoLocationApiClient _geoLocationClient;
        private readonly IRepositoryApiClient _repositoryApiClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly IProxyCheckService _proxyCheckService;
        private readonly ILogger<IPAddressesController> _logger;

        public IPAddressesController(
            IGeoLocationApiClient geoLocationClient,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            IProxyCheckService proxyCheckService,
            ILogger<IPAddressesController> logger)
        {
            _geoLocationClient = geoLocationClient ?? throw new ArgumentNullException(nameof(geoLocationClient));
            _repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _proxyCheckService = proxyCheckService ?? throw new ArgumentNullException(nameof(proxyCheckService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return NotFound();

            var viewModel = new IPAddressDetailsViewModel
            {
                IpAddress = ipAddress
            };

            try
            {
                // Get GeoLocation information
                var getGeoLocationResult = await _geoLocationClient.GeoLookup.V1.GetGeoLocation(ipAddress);
                if (getGeoLocationResult.IsSuccess && getGeoLocationResult.Result?.Data != null)
                    viewModel.GeoLocation = getGeoLocationResult.Result.Data;

                // Get ProxyCheck information
                var proxyCheckResult = await _proxyCheckService.GetIpRiskDataAsync(ipAddress);
                viewModel.ProxyCheck = proxyCheckResult;

                // Get players who have used this IP address
                var playersResponse = await _repositoryApiClient.Players.V1.GetPlayersWithIpAddress(ipAddress, 0, 100, PlayersOrder.LastSeenDesc, PlayerEntityOptions.None);
                if (playersResponse.IsSuccess && playersResponse.Result != null)
                {
                    viewModel.Players = playersResponse.Result.Data.Items;
                    viewModel.TotalPlayersCount = playersResponse.Result.Data.TotalCount;
                }
                else
                {
                    _logger.LogError("Failed to retrieve players with IP address {IpAddress}", ipAddress);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving IP address details for {IpAddress}", ipAddress);
                return RedirectToAction("Display", "Errors", new { id = 500 });
            }
        }
    }
}
