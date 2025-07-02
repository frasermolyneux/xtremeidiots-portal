using MX.GeoLocation.LookupApi.Abstractions.Models;
using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    /// <summary>
    /// View model that combines a player IP address with its geolocation and proxy check data
    /// </summary>
    public class PlayerIpAddressViewModel
    {
        /// <summary>
        /// The base IP address data
        /// </summary>
        public IpAddressDto IpAddressDto { get; set; } = null!;

        /// <summary>
        /// The geolocation data for this IP address
        /// </summary>
        public GeoLocationDto? GeoLocation { get; set; }

        /// <summary>
        /// The proxy check data for this IP address
        /// </summary>
        public ProxyCheckResult? ProxyCheck { get; set; }

        /// <summary>
        /// Helper property to get the IP address string
        /// </summary>
        public string Address => IpAddressDto?.Address ?? string.Empty;

        /// <summary>
        /// Helper property to check if this is the player's current IP address
        /// </summary>
        public bool IsCurrentIp { get; set; }

        /// <summary>
        /// The risk score from ProxyCheck
        /// </summary>
        public int RiskScore => ProxyCheck?.RiskScore ?? 0;

        /// <summary>
        /// Whether this IP is a proxy
        /// </summary>
        public bool IsProxy => ProxyCheck?.IsProxy ?? false;

        /// <summary>
        /// Whether this IP is a VPN
        /// </summary>
        public bool IsVpn => ProxyCheck?.IsVpn ?? false;

        /// <summary>
        /// The type of proxy, if applicable
        /// </summary>
        public string ProxyType => ProxyCheck?.Type ?? string.Empty;

        /// <summary>
        /// The country code for this IP, or "unknown" if not available
        /// </summary>
        public string CountryCode => GeoLocation?.CountryCode ?? "unknown";
    }
}
