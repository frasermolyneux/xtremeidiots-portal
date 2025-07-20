
using MX.GeoLocation.Abstractions.Models.V1;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class PlayerDetailsViewModel
{
    public PlayerDetailsViewModel()
    {
        EnrichedIpAddresses = [];
    }

    public PlayerDto? Player { get; set; }
    public GeoLocationDto? GeoLocation { get; set; }

    public List<PlayerIpAddressViewModel> EnrichedIpAddresses { get; set; }
}