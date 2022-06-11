using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IReportsApi
    {
        Task<ApiResponseDto<ReportDto>> GetReport(Guid reportId);
        Task<ApiResponseDto<ReportsCollectionDto>> GetReports(GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filter, int skipEntries, int takeEntries, ReportsOrder? order);

        Task<ApiResponseDto> CreateReports(List<CreateReportDto> createReportDtos);

        Task<ApiResponseDto> CloseReport(Guid reportId, CloseReportDto closeReportDto);
    }
}