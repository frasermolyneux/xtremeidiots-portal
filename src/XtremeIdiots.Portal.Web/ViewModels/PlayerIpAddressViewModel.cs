using MX.GeoLocation.Abstractions.Models.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Web.Services;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for displaying player IP address information with geo-location and proxy check data
/// </summary>
public class PlayerIpAddressViewModel
{
    /// <summary>
    /// The IP address data transfer object
    /// </summary>
    public IpAddressDto IpAddressDto { get; set; } = null!;

    /// <summary>
    /// Geographic location information for the IP address
    /// </summary>
    public GeoLocationDto? GeoLocation { get; set; }

    /// <summary>
    /// Proxy check results for the IP address
    /// </summary>
    public ProxyCheckResult? ProxyCheck { get; set; }

    /// <summary>
    /// The IP address string
    /// </summary>
    public string Address => IpAddressDto?.Address ?? string.Empty;

    /// <summary>
    /// Indicates if this is the player's current IP address
    /// </summary>
    public bool IsCurrentIp { get; set; }

    /// <summary>
    /// Risk score from proxy check (0-100)
    /// </summary>
    public int RiskScore => ProxyCheck?.RiskScore ?? 0;

    /// <summary>
    /// Indicates if the IP address is identified as a proxy
    /// </summary>
    public bool IsProxy => ProxyCheck?.IsProxy ?? false;

    /// <summary>
    /// Indicates if the IP address is identified as a VPN
    /// </summary>
    public bool IsVpn => ProxyCheck?.IsVpn ?? false;

    /// <summary>
    /// The type of proxy if identified
    /// </summary>
    public string ProxyType => ProxyCheck?.Type ?? string.Empty;

    /// <summary>
    /// The country code from geo-location data
    /// </summary>
    public string CountryCode => GeoLocation?.CountryCode ?? "unknown";
}