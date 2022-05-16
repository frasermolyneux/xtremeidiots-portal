using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.AdminActionsApi
{
    public class AdminActionsApiClient : BaseApiClient, IAdminActionsApiClient
    {
        public AdminActionsApiClient(ILogger<AdminActionsApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<List<AdminActionDto>> GetAdminActions(string gameType, Guid? playerId, string adminId, string filterType, int skipEntries, int takeEntries, string order)
        {
            var request = await CreateRequest($"repository/admin-actions", Method.Get);

            if (!string.IsNullOrWhiteSpace(gameType))
                request.AddQueryParameter("gameType", gameType);

            if (playerId != null)
                request.AddQueryParameter("playerId", playerId.ToString());

            if (!string.IsNullOrWhiteSpace(adminId))
                request.AddQueryParameter("adminId", adminId);

            if (!string.IsNullOrWhiteSpace(filterType))
                request.AddQueryParameter("filterType", filterType);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (!string.IsNullOrWhiteSpace(order))
                request.AddQueryParameter("order", order);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<AdminActionDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<AdminActionDto> GetAdminAction(Guid adminActionId)
        {
            var request = await CreateRequest($"repository/admin-actions/{adminActionId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<AdminActionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteAdminAction(Guid adminActionId)
        {
            var request = await CreateRequest($"repository/admin-actions/{adminActionId}", Method.Delete);
            await ExecuteAsync(request);
        }
    }
}
