using MX.GeoLocation.LookupApi.Abstractions.Models;

using XtremeIdiots.Portal.Web.Services;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class IPAddressDetailsViewModel
    {
        /// <summary>
        /// The IP address being displayed
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// The GeoLocation data for the IP address
        /// </summary>
        public GeoLocationDto? GeoLocation { get; set; }

        /// <summary>
        /// The ProxyCheck data for the IP address
        /// </summary>
        public ProxyCheckResult? ProxyCheck { get; set; }

        /// <summary>
        /// The count of players who have used this IP address
        /// </summary>
        public int TotalPlayersCount { get; set; }

        /// <summary>
        /// The list of players who have used this IP address
        /// </summary>
        public IEnumerable<PlayerDto> Players { get; set; } = Enumerable.Empty<PlayerDto>();
    }
}
