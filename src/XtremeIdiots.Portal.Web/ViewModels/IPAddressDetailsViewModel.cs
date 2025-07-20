using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using MX.GeoLocation.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class IPAddressDetailsViewModel
{

    public string IpAddress { get; set; } = string.Empty;

    public GeoLocationDto? GeoLocation { get; set; }

    public ProxyCheckResult? ProxyCheck { get; set; }

    public int TotalPlayersCount { get; set; }

    public IEnumerable<PlayerDto> Players { get; set; } = [];
}