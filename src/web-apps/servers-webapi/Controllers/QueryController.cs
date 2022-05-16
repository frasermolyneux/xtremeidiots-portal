using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models;
using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class QueryController : Controller
    {
        private readonly ILogger<QueryController> logger;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IQueryClientFactory queryClientFactory;

        public QueryController(
            ILogger<QueryController> logger,
            IRepositoryApiClient repositoryApiClient,
            IQueryClientFactory queryClientFactory)
        {
            this.logger = logger;

            this.repositoryApiClient = repositoryApiClient;
            this.queryClientFactory = queryClientFactory;
        }

        [HttpGet]
        [Route("api/query/{serverId}/status")]
        public async Task<IActionResult> GetServerStatus(Guid serverId)
        {

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(serverId);

            if (gameServerDto == null)
                return NotFound();

            var queryClient = queryClientFactory.CreateInstance(gameServerDto.GameType, gameServerDto.Hostname, gameServerDto.QueryPort);

            try
            {
                var statusResult = await queryClient.GetServerStatus();

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
                logger.LogError(ex, $"Failed to get status from server {gameServerDto.Hostname}:{gameServerDto.QueryPort}");
                throw;
            }
        }
    }
}
