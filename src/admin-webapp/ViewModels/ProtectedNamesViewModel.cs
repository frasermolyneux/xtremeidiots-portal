using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
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