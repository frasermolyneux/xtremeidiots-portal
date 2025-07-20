using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for displaying protected name usage reports
/// </summary>
public class ProtectedNameReportViewModel
{
    /// <summary>
    /// Gets or sets the protected name usage report data
    /// </summary>
    public ProtectedNameUsageReportDto? Report { get; set; }
}