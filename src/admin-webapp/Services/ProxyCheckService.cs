using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XtremeIdiots.Portal.AdminWebApp.Services
{
    /// <summary>
    /// Service to interact with the ProxyCheck.io API for IP risk assessment.
    /// </summary>
    public interface IProxyCheckService
    {
        /// <summary>
        /// Gets risk data for the specified IP address.
        /// </summary>
        /// <param name="ipAddress">The IP address to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ProxyCheck risk data.</returns>
        Task<ProxyCheckResult> GetIpRiskDataAsync(string ipAddress, CancellationToken cancellationToken = default);
    }
    public class ProxyCheckService : IProxyCheckService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ProxyCheckService> _logger;
        private readonly string? _apiKey;
        private readonly string _apiBaseUrl = "https://proxycheck.io/v2/";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        /// <summary>
        /// Creates a new instance of ProxyCheckService.
        /// </summary>
        /// <param name="httpClientFactory">HTTP client factory.</param>
        /// <param name="memoryCache">Memory cache for storing API results.</param>
        /// <param name="configuration">Application configuration for retrieving API key.</param>
        /// <param name="logger">Logger.</param>
        public ProxyCheckService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<ProxyCheckService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // Get API key from configuration
            _apiKey = configuration["ProxyCheck:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
            {
                logger.LogWarning("ProxyCheck:ApiKey is not configured. ProxyCheck service will not be able to make API calls.");
            }
        }        /// <inheritdoc/>
        public async Task<ProxyCheckResult> GetIpRiskDataAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                _logger.LogWarning("Empty IP address provided for ProxyCheck");
                return new ProxyCheckResult { IsError = true, ErrorMessage = "Invalid IP address" };
            }

            // Try to get from cache first
            var cacheKey = $"ProxyCheck_{ipAddress}";
            if (_memoryCache.TryGetValue(cacheKey, out ProxyCheckResult? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Retrieved ProxyCheck data from cache for IP {IpAddress}", ipAddress);
                return cachedResult;
            }

            // If API key is not configured, return a dummy result
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("ProxyCheck API key is not configured. Returning dummy result for IP {IpAddress}", ipAddress);
                var dummyResult = new ProxyCheckResult
                {
                    IsError = true,
                    ErrorMessage = "API key not configured",
                    IpAddress = ipAddress
                };
                return dummyResult;
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Build the API URL with parameters
                var apiUrl = $"{_apiBaseUrl}{ipAddress}?key={_apiKey}&vpn=1&asn=1&risk=1&seen=1&tag=portal";

                _logger.LogDebug("Calling ProxyCheck.io API for IP {IpAddress}", ipAddress);
                var response = await httpClient.GetAsync(apiUrl, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("ProxyCheck API error for {IpAddress}: {StatusCode} - {Response}",
                        ipAddress, response.StatusCode, responseContent);
                    return new ProxyCheckResult { IsError = true, ErrorMessage = $"API Error: {response.StatusCode}" };
                }

                // Parse the response content
                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;

                if (root.TryGetProperty("status", out var statusElement) && statusElement.GetString() == "ok")
                {
                    if (root.TryGetProperty(ipAddress, out var ipElement))
                    {
                        var result = new ProxyCheckResult
                        {
                            IsError = false,
                            IsProxy = ipElement.TryGetProperty("proxy", out var proxyElement) && proxyElement.GetString() == "yes",
                            IsVpn = ipElement.TryGetProperty("type", out var typeElement) && typeElement.GetString()?.ToLower() == "vpn",
                            RiskScore = ipElement.TryGetProperty("risk", out var riskElement) ? riskElement.GetInt32() : 0,
                            Country = ipElement.TryGetProperty("country", out var countryElement) ? countryElement.GetString() ?? string.Empty : string.Empty,
                            Region = ipElement.TryGetProperty("region", out var regionElement) ? regionElement.GetString() ?? string.Empty : string.Empty,
                            AsNumber = ipElement.TryGetProperty("asn", out var asnElement) ? asnElement.GetString() ?? string.Empty : string.Empty,
                            AsOrganization = ipElement.TryGetProperty("provider", out var providerElement) ? providerElement.GetString() ?? string.Empty : string.Empty,
                            IpAddress = ipAddress
                        };

                        // Cache the result
                        _memoryCache.Set(cacheKey, result, _cacheDuration);
                        return result;
                    }
                }

                _logger.LogWarning("Failed to parse ProxyCheck response for IP {IpAddress}: {Response}", ipAddress, responseContent);
                return new ProxyCheckResult { IsError = true, ErrorMessage = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ProxyCheck.io API for IP {IpAddress}", ipAddress);
                return new ProxyCheckResult { IsError = true, ErrorMessage = $"Exception: {ex.Message}" };
            }
        }
    }

    /// <summary>
    /// Result from the ProxyCheck.io API.
    /// </summary>
    public class ProxyCheckResult
    {
        /// <summary>
        /// The IP address that was checked.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// True if an error occurred during the API call.
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Error message if an error occurred.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// True if the IP is a proxy.
        /// </summary>
        public bool IsProxy { get; set; }

        /// <summary>
        /// True if the IP is a VPN.
        /// </summary>
        public bool IsVpn { get; set; }

        /// <summary>
        /// Risk score from 0-100 where higher values indicate higher risk.
        /// </summary>
        public int RiskScore { get; set; }

        /// <summary>
        /// Country of the IP address.
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Region of the IP address.
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// AS number for the IP address.
        /// </summary>
        public string AsNumber { get; set; } = string.Empty;

        /// <summary>
        /// AS organization for the IP address.
        /// </summary>
        public string AsOrganization { get; set; } = string.Empty;

        /// <summary>
        /// Gets a CSS class based on the risk score for color-coding.
        /// </summary>
        public string GetRiskClass()
        {
            return RiskScore switch
            {
                >= 80 => "text-bg-danger",
                >= 50 => "text-bg-warning",
                >= 25 => "text-bg-info",
                _ => "text-bg-success"
            };
        }
    }
}
