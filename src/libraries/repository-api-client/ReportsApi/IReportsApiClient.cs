using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;

namespace XtremeIdiots.Portal.RepositoryApiClient.ReportsApi
{
    public interface IReportsApiClient
    {
        Task<ReportDto?> GetReport(Guid reportId);
        Task<ReportsCollectionDto?> GetReports(GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filterType, int skipEntries, int takeEntries, ReportsOrder? order);
        Task<ReportsCollectionDto?> CreateReports(List<CreateReportDto> createReportDtos);
        Task<ReportDto?> CloseReport(Guid reportId, CloseReportDto closeReportDto);
    }
}