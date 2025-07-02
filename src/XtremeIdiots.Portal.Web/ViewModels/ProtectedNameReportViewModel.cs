using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class ProtectedNameReportViewModel
    {
        public ProtectedNameReportViewModel()
        {
        }

        public ProtectedNameUsageReportDto? Report { get; set; }
    }
}