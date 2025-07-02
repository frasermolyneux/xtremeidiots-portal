using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

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