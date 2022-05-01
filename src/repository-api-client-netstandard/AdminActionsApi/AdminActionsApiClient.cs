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
        public AdminActionsApiClient(ILogger<AdminActionsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<List<AdminActionDto>> GetAdminActions(string accessToken, string gameType, Guid? playerId, string adminId, string filterType, int skipEntries, int takeEntries, string order)
        {
            var request = CreateRequest($"repository/admin-actions", Method.GET, accessToken);

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

        public async Task<AdminActionDto> GetAdminAction(string accessToken, Guid adminActionId)
        {
            var request = CreateRequest($"repository/admin-actions/{adminActionId}", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<AdminActionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteAdminAction(string accessToken, Guid adminActionId)
        {
            var request = CreateRequest($"repository/admin-actions/{adminActionId}", Method.DELETE, accessToken);
            await ExecuteAsync(request);
        }
    }
}
