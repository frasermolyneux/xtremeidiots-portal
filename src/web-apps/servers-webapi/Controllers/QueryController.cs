using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models;
using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class QueryController : Controller
    {
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IQueryClientFactory queryClientFactory;
        private readonly TelemetryClient telemetryClient;
        private readonly IMemoryCache memoryCache;

        public QueryController(
            IRepositoryApiClient repositoryApiClient,
            IQueryClientFactory queryClientFactory,
            TelemetryClient telemetryClient,
            IMemoryCache memoryCache)
        {
            this.repositoryApiClient = repositoryApiClient;
            this.queryClientFactory = queryClientFactory;
            this.telemetryClient = telemetryClient;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("api/query/{serverId}/status")]
        public async Task<IActionResult> GetServerStatus(Guid serverId)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(serverId);

            if (gameServerApiResponse.IsNotFound)
                return NotFound();

            var queryClient = queryClientFactory.CreateInstance(gameServerApiResponse.Result.GameType, gameServerApiResponse.Result.Hostname, gameServerApiResponse.Result.QueryPort);

            var operation = telemetryClient.StartOperation<DependencyTelemetry>("QueryServerStatus");
            operation.Telemetry.Type = $"{gameServerApiResponse.Result.GameType}Server";
            operation.Telemetry.Target = $"{gameServerApiResponse.Result.Hostname}:{gameServerApiResponse.Result.QueryPort}";

            try
            {
                if (!memoryCache.TryGetValue($"{gameServerApiResponse.Result.Id}-query-status", out IQueryResponse statusResult))
                {
                    statusResult = await queryClient.GetServerStatus();

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));

                    memoryCache.Set($"{gameServerApiResponse.Result.Id}-query-status", statusResult, cacheEntryOptions);
                }

                if (statusResult != null)
                {
                    var dto = new ServerQueryStatusResponseDto
                    {
                        ServerName = statusResult.ServerName,
                        Map = statusResult.Map,
                        Mod = statusResult.Mod,
                        MaxPlayers = statusResult.MaxPlayers,
                        PlayerCount = statusResult.PlayerCount,
                        ServerParams = statusResult.ServerParams,
                        Players = statusResult.Players.Select(p => new ServerQueryPlayerDto
                        {
                            Name = p.Name,
                            Score = p.Score
                        }).ToList()
                    };

                    return new OkObjectResult(dto);
                }
                else
                {
                    return new NoContentResult();
                }
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = ex.Message;
                telemetryClient.TrackException(ex);

                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }
    }
}
