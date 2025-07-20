using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class ProtectedNameReportViewModel
{
    public ProtectedNameReportViewModel()
    {
    }

    public ProtectedNameUsageReportDto? Report { get; set; }
}