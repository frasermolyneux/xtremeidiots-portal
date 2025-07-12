using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class ProtectedNamesViewModel
    {
        public ProtectedNamesViewModel()
        {
            ProtectedNames = new List<ProtectedNameDto>();
        }

        public List<ProtectedNameDto> ProtectedNames { get; set; }
    }
}