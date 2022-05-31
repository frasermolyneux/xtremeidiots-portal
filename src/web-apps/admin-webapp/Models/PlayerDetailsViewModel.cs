using FM.GeoLocation.Contract.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.AdminWebApp.Models
{
    public class PlayerDetailsViewModel
    {
        public PlayerDto Player { get; set; }
        public GeoLocationDto GeoLocation { get; set; }
        public List<AdminActionDto> AdminActions { get; set; }
    }
}
