using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api;

public class GameServersEventsApi : BaseApi, IGameServersEventsApi
{
    public GameServersEventsApi(ILogger<GameServersEventsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
    {
    }

    public async Task<ApiResponseDto> CreateGameServerEvent(CreateGameServerEventDto createGameServerEventDto)
    {
        var request = await CreateRequest($"game-server-events", Method.Post);
        request.AddJsonBody(new List<CreateGameServerEventDto> { createGameServerEventDto });

        var response = await ExecuteAsync(request);

        return response.ToApiResponse();
    }

    public async Task<ApiResponseDto> CreateGameServerEvents(List<CreateGameServerEventDto> createGameServerEventDtos)
    {
        var request = await CreateRequest($"game-server-events", Method.Post);
        request.AddJsonBody(createGameServerEventDtos);

        var response = await ExecuteAsync(request);

        return response.ToApiResponse();
    }
}