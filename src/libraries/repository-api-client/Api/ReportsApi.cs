using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class ReportsApi : BaseApi, IReportsApi
    {
        public ReportsApi(ILogger<ReportsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto<ReportDto>> GetReport(Guid reportId)
        {
            var request = await CreateRequest($"repository/reports/{reportId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<ReportDto>();
        }

        public async Task<ApiResponseDto<ReportsCollectionDto>> GetReports(GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filter, int skipEntries, int takeEntries, ReportsOrder? order)
        {
            var request = await CreateRequest("repository/reports", Method.Get);

            if (gameType.HasValue)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (serverId.HasValue)
                request.AddQueryParameter("serverId", serverId.ToString());

            if (cutoff.HasValue)
                request.AddQueryParameter("cutoff", cutoff.Value.ToString("MM/dd/yyyy HH:mm:ss"));

            if (filter.HasValue)
                request.AddQueryParameter("filter", filter.ToString());

            request.AddQueryParameter("skipEntries", skipEntries.ToString());
            request.AddQueryParameter("takeEntries", takeEntries.ToString());

            if (order.HasValue)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<ReportsCollectionDto>();
        }

        public async Task<ApiResponseDto> CreateReports(List<CreateReportDto> createReportDtos)
        {
            var request = await CreateRequest("repository/reports", Method.Post);
            request.AddJsonBody(createReportDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> CloseReport(Guid reportId, CloseReportDto closeReportDto)
        {
            var request = await CreateRequest($"repository/reports/{reportId}/close", Method.Post);
            request.AddJsonBody(closeReportDto);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
