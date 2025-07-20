using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XtremeIdiots.Portal.Web.Services
{

    public interface IProxyCheckService
    {

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

        public ProxyCheckService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<ProxyCheckService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _apiKey = configuration["ProxyCheck:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
            {
                logger.LogWarning("ProxyCheck:ApiKey is not configured. ProxyCheck service will not be able to make API calls.");
            }
        }
        public async Task<ProxyCheckResult> GetIpRiskDataAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                _logger.LogWarning("Empty IP address provided for ProxyCheck");
                return new ProxyCheckResult { IsError = true, ErrorMessage = "Invalid IP address" };
            }

            var cacheKey = $"ProxyCheck_{ipAddress}";
            if (_memoryCache.TryGetValue(cacheKey, out ProxyCheckResult? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Retrieved ProxyCheck data from cache for IP {IpAddress}", ipAddress);
                return cachedResult;
            }

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
                            Type = ipElement.TryGetProperty("type", out var typeValue) ? typeValue.GetString() ?? string.Empty : string.Empty,
                            RiskScore = ipElement.TryGetProperty("risk", out var riskElement) ? riskElement.GetInt32() : 0,
                            Country = ipElement.TryGetProperty("country", out var countryElement) ? countryElement.GetString() ?? string.Empty : string.Empty,
                            Region = ipElement.TryGetProperty("region", out var regionElement) ? regionElement.GetString() ?? string.Empty : string.Empty,
                            AsNumber = ipElement.TryGetProperty("asn", out var asnElement) ? asnElement.GetString() ?? string.Empty : string.Empty,
                            AsOrganization = ipElement.TryGetProperty("provider", out var providerElement) ? providerElement.GetString() ?? string.Empty : string.Empty,
                            IpAddress = ipAddress
                        };

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

    public class ProxyCheckResult
    {

        public string IpAddress { get; set; } = string.Empty;

        public bool IsError { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsProxy { get; set; }

        public bool IsVpn { get; set; }

        public string Type { get; set; } = string.Empty;

        public int RiskScore { get; set; }

        public string Country { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public string AsNumber { get; set; } = string.Empty;

        public string AsOrganization { get; set; } = string.Empty;

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