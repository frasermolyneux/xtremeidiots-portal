using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace XtremeIdiots.Portal.Web.Services;

/// <summary>
/// Interface for checking IP address risk and proxy status using ProxyCheck.io service
/// </summary>
public interface IProxyCheckService
{
    /// <summary>
    /// Gets IP address risk data including proxy, VPN detection and geographic information
    /// </summary>
    /// <param name="ipAddress">The IP address to check</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>ProxyCheck result containing risk assessment and geographic data</returns>
    Task<ProxyCheckResult> GetIpRiskDataAsync(string ipAddress, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for checking IP address risk and proxy status using ProxyCheck.io API
/// </summary>
public class ProxyCheckService : IProxyCheckService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IMemoryCache memoryCache;
    private readonly ILogger<ProxyCheckService> logger;
    private readonly string? apiKey;
    private readonly string apiBaseUrl = "https://proxycheck.io/v2/";
    private readonly TimeSpan cacheDuration = TimeSpan.FromHours(1);

    /// <summary>
    /// Initializes a new instance of the ProxyCheckService
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients</param>
    /// <param name="memoryCache">Memory cache for storing results</param>
    /// <param name="configuration">Application configuration containing API key</param>
    /// <param name="logger">Logger instance for this service</param>
    public ProxyCheckService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IConfiguration configuration,
        ILogger<ProxyCheckService> logger)
    {
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        apiKey = configuration["ProxyCheck:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("ProxyCheck:ApiKey is not configured. ProxyCheck service will not be able to make API calls.");
        }
    }

    /// <summary>
    /// Gets IP address risk data including proxy, VPN detection and geographic information
    /// </summary>
    /// <param name="ipAddress">The IP address to check</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>ProxyCheck result containing risk assessment and geographic data</returns>
    public async Task<ProxyCheckResult> GetIpRiskDataAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            logger.LogWarning("Empty IP address provided for ProxyCheck");
            return new ProxyCheckResult { IsError = true, ErrorMessage = "Invalid IP address" };
        }

        var cacheKey = $"ProxyCheck_{ipAddress}";
        if (memoryCache.TryGetValue(cacheKey, out ProxyCheckResult? cachedResult) && cachedResult is not null)
        {
            logger.LogDebug("Retrieved ProxyCheck data from cache for IP {IpAddress}", ipAddress);
            return cachedResult;
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("ProxyCheck API key is not configured. Returning dummy result for IP {IpAddress}", ipAddress);
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
            var httpClient = httpClientFactory.CreateClient();
            var apiUrl = $"{apiBaseUrl}{ipAddress}?key={apiKey}&vpn=1&asn=1&risk=1&seen=1&tag=portal";

            logger.LogDebug("Calling ProxyCheck.io API for IP {IpAddress}", ipAddress);
            var response = await httpClient.GetAsync(apiUrl, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("ProxyCheck API error for {IpAddress}: {StatusCode} - {Response}",
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

                    memoryCache.Set(cacheKey, result, cacheDuration);
                    return result;
                }
            }

            logger.LogWarning("Failed to parse ProxyCheck response for IP {IpAddress}: {Response}", ipAddress, responseContent);
            return new ProxyCheckResult { IsError = true, ErrorMessage = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling ProxyCheck.io API for IP {IpAddress}", ipAddress);
            return new ProxyCheckResult { IsError = true, ErrorMessage = $"Exception: {ex.Message}" };
        }
    }
}

/// <summary>
/// Result object containing IP address risk assessment and geographic information
/// </summary>
public class ProxyCheckResult
{
    /// <summary>
    /// The IP address that was checked
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if an error occurred during the check
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// Error message if an error occurred
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the IP address is identified as a proxy
    /// </summary>
    public bool IsProxy { get; set; }

    /// <summary>
    /// Indicates if the IP address is identified as a VPN
    /// </summary>
    public bool IsVpn { get; set; }

    /// <summary>
    /// The type of connection (proxy, VPN, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Risk score from 0-100, with higher scores indicating higher risk
    /// </summary>
    public int RiskScore { get; set; }

    /// <summary>
    /// Country where the IP address is located
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Region/state where the IP address is located
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Autonomous System Number associated with the IP address
    /// </summary>
    public string AsNumber { get; set; } = string.Empty;

    /// <summary>
    /// Organization that owns the Autonomous System
    /// </summary>
    public string AsOrganization { get; set; } = string.Empty;

    /// <summary>
    /// Gets the CSS class for displaying risk level based on risk score
    /// </summary>
    /// <returns>Bootstrap CSS class for risk level visualization</returns>
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