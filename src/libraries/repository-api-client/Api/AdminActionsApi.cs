using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class AdminActionsApi : BaseApi, IAdminActionsApi
    {
        public AdminActionsApi(ILogger<AdminActionsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto<AdminActionDto>> GetAdminAction(Guid adminActionId)
        {
            var request = await CreateRequest($"admin-actions/{adminActionId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<AdminActionDto>();
        }

        public async Task<ApiResponseDto<AdminActionCollectionDto>> GetAdminActions(GameType? gameType, Guid? playerId, string? adminId, AdminActionFilter? filter, int skipEntries, int takeEntries, AdminActionOrder? order)
        {
            var request = await CreateRequest($"admin-actions", Method.Get);

            if (gameType.HasValue)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (playerId.HasValue)
                request.AddQueryParameter("playerId", playerId.ToString());

            if (!string.IsNullOrWhiteSpace(adminId))
                request.AddQueryParameter("adminId", adminId);

            if (filter.HasValue)
                request.AddQueryParameter("filter", filter.ToString());

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (order.HasValue)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<AdminActionCollectionDto>();
        }

        public async Task<ApiResponseDto> CreateAdminAction(CreateAdminActionDto createAdminActionDto)
        {
            var request = await CreateRequest($"admin-actions", Method.Post);
            request.AddJsonBody(createAdminActionDto);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateAdminAction(EditAdminActionDto editAdminActionDto)
        {
            var request = await CreateRequest($"admin-actions/{editAdminActionDto.AdminActionId}", Method.Patch);
            request.AddJsonBody(editAdminActionDto);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> DeleteAdminAction(Guid adminActionId)
        {
            var request = await CreateRequest($"admin-actions/{adminActionId}", Method.Delete);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
