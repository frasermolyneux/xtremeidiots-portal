using FM.GeoLocation.Contract.Models;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class PlayerDetailsViewModel
    {
        public PlayerDto Player { get; set; }
        public GeoLocationDto GeoLocation { get; set; }
        public List<AdminActionDto> AdminActions { get; set; }
    }
}
