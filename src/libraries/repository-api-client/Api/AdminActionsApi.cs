using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using System.Net;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class AdminActionsApi : BaseApi, IAdminActionsApi
    {
        public AdminActionsApi(ILogger<AdminActionsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<List<AdminActionDto>?> GetAdminActions(GameType? gameType, Guid? playerId, string? adminId, AdminActionFilter? filterType, int skipEntries, int takeEntries, AdminActionOrder? order)
        {
            var request = await CreateRequest($"repository/admin-actions", Method.Get);

            if (gameType != null)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (playerId != null)
                request.AddQueryParameter("playerId", playerId.ToString());

            if (!string.IsNullOrWhiteSpace(adminId))
                request.AddQueryParameter("adminId", adminId);

            if (filterType != null)
                request.AddQueryParameter("filterType", filterType.ToString());

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (order != null)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<AdminActionDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<AdminActionDto?> GetAdminAction(Guid adminActionId)
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
