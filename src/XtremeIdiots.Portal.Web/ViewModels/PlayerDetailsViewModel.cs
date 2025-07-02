
using System.Collections.Generic;
using MX.GeoLocation.LookupApi.Abstractions.Models;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class PlayerDetailsViewModel
    {
        public PlayerDetailsViewModel()
        {
            EnrichedIpAddresses = new List<PlayerIpAddressViewModel>();
        }

        public PlayerDto? Player { get; set; }
        public GeoLocationDto? GeoLocation { get; set; }

        /// <summary>
        /// A collection of IP addresses with their associated geolocation and proxy check data
        /// </summary>
        public List<PlayerIpAddressViewModel> EnrichedIpAddresses { get; set; }
    }
}
