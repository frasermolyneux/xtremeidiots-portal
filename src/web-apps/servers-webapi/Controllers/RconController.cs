using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models;
using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class RconController : Controller
    {
        private readonly ILogger<RconController> logger;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRconClientFactory rconClientFactory;

        public RconController(
            ILogger<RconController> logger,
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient,
            IRconClientFactory rconClientFactory
            )
        {
            this.logger = logger;
            this.repositoryTokenProvider = repositoryTokenProvider;
            this.repositoryApiClient = repositoryApiClient;
            this.rconClientFactory = rconClientFactory;
        }

        [HttpGet]
        [Route("api/rcon/{serverId}/players")]
        public async Task<IActionResult> GetServerStatus(Guid serverId)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, serverId);

            if (gameServerDto == null)
                return NotFound();

            var queryClient = rconClientFactory.CreateInstance(gameServerDto.GameType, gameServerDto.Id, gameServerDto.Hostname, gameServerDto.QueryPort, gameServerDto.RconPassword);

            try
            {
                var statusResult = queryClient.GetPlayers();

                if (statusResult != null)
                {
                    var dto = new ServerRconStatusResponseDto
                    {
                        Players = statusResult.Select(p => new ServerRconPlayerDto
                        {
                            Num = p.Num,
                            Guid = p.Guid,
                            Name = p.Name,
                            IpAddress = p.IpAddress,
                            Rate = p.Rate
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
