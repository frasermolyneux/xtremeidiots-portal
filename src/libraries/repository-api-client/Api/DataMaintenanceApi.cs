
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api;

public class DataMaintenanceApi : BaseApi, IDataMaintenanceApi
{
    public DataMaintenanceApi(ILogger<DataMaintenanceApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
    {

    }

    public async Task<ApiResponseDto> PruneChatMessages()
    {
        var response = await ExecuteAsync(await CreateRequest("data-maintenance/prune-chat-messages", Method.Delete));

        return response.ToApiResponse();
    }

    public async Task<ApiResponseDto> PruneGameServerEvents()
    {
        var response = await ExecuteAsync(await CreateRequest("data-maintenance/prune-game-server-events", Method.Delete));

        return response.ToApiResponse();
    }

    public async Task<ApiResponseDto> PruneGameServerStats()
    {
        var response = await ExecuteAsync(await CreateRequest("data-maintenance/prune-game-server-stats", Method.Delete));

        return response.ToApiResponse();
    }

    public async Task<ApiResponseDto> PruneRecentPlayers()
    {
        var response = await ExecuteAsync(await CreateRequest("data-maintenance/prune-recent-players", Method.Delete));

        return response.ToApiResponse();
    }
}