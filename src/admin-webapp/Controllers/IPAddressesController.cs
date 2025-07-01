using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MX.GeoLocation.GeoLocationApi.Client;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Services;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
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
                var geoLocation = await _geoLocationClient.GeoLookup.GetGeoLocation(ipAddress);
                if (geoLocation.IsSuccess && geoLocation.Result != null)
                    viewModel.GeoLocation = geoLocation.Result;
                else
                {
                    geoLocation.Errors.ForEach(ex => _telemetryClient.TrackException(new ApplicationException(ex)));
                }

                // Get ProxyCheck information
                var proxyCheckResult = await _proxyCheckService.GetIpRiskDataAsync(ipAddress);
                viewModel.ProxyCheck = proxyCheckResult;

                // Get players who have used this IP address
                var playersResponse = await _repositoryApiClient.Players.V1.GetPlayersWithIpAddress(ipAddress, 0, 100, PlayersOrder.LastSeenDesc, PlayerEntityOptions.None);
                if (playersResponse.IsSuccess && playersResponse.Result != null)
                {
                    viewModel.Players = playersResponse.Result.Entries;
                    viewModel.TotalPlayersCount = playersResponse.Result.TotalRecords;
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
