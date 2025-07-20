using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using MX.GeoLocation.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class PlayerIpAddressViewModel
{

    public IpAddressDto IpAddressDto { get; set; } = null!;

    public GeoLocationDto? GeoLocation { get; set; }

    public ProxyCheckResult? ProxyCheck { get; set; }

    public string Address => IpAddressDto?.Address ?? string.Empty;

    public bool IsCurrentIp { get; set; }

    public int RiskScore => ProxyCheck?.RiskScore ?? 0;

    public bool IsProxy => ProxyCheck?.IsProxy ?? false;

    public bool IsVpn => ProxyCheck?.IsVpn ?? false;

    public string ProxyType => ProxyCheck?.Type ?? string.Empty;

    public string CountryCode => GeoLocation?.CountryCode ?? "unknown";
}