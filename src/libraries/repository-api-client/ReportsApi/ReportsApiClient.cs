using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;

namespace XtremeIdiots.Portal.RepositoryApiClient.ReportsApi
{
    public class ReportsApiClient : BaseApiClient, IReportsApiClient
    {
        public ReportsApiClient(ILogger<ReportsApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ReportDto?> GetReport(Guid reportId)
        {
            var request = await CreateRequest($"repository/reports/{reportId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ReportDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<ReportsCollectionDto?> GetReports(GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filterType, int skipEntries, int takeEntries, ReportsOrder? order)
        {
            var request = await CreateRequest("repository/reports", Method.Get);

            if (gameType.HasValue)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (serverId.HasValue)
                request.AddQueryParameter("serverId", serverId.ToString());

            if (cutoff.HasValue)
                request.AddQueryParameter("cutoff", cutoff.ToString());

            if (filterType.HasValue)
                request.AddQueryParameter("filterType", filterType.ToString());

            request.AddQueryParameter("skipEntries", skipEntries.ToString());
            request.AddQueryParameter("takeEntries", takeEntries.ToString());

            if (order != null)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ReportsCollectionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<ReportsCollectionDto?> CreateReports(List<CreateReportDto> createReportDtos)
        {
            var request = await CreateRequest("repository/reports", Method.Post);
            request.AddJsonBody(createReportDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ReportsCollectionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<ReportDto?> CloseReport(Guid reportId, CloseReportDto closeReportDto)
        {
            var request = await CreateRequest($"repository/reports/{reportId}/close", Method.Post);
            request.AddJsonBody(closeReportDto);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ReportDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
