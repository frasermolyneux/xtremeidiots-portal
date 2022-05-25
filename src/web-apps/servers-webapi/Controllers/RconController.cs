using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRconClientFactory rconClientFactory;
        private readonly TelemetryClient telemetryClient;

        public RconController(
            ILogger<RconController> logger,
            IRepositoryApiClient repositoryApiClient,
            IRconClientFactory rconClientFactory,
            TelemetryClient telemetryClient)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
            this.rconClientFactory = rconClientFactory;
            this.telemetryClient = telemetryClient;
        }

        [HttpGet]
        [Route("api/rcon/{serverId}/status")]
        public async Task<IActionResult> GetServerStatus(Guid serverId)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(serverId);

            if (gameServerDto == null)
                return NotFound();

            var queryClient = rconClientFactory.CreateInstance(gameServerDto.GameType, gameServerDto.Id, gameServerDto.Hostname, gameServerDto.QueryPort, gameServerDto.RconPassword);

            var operation = telemetryClient.StartOperation<DependencyTelemetry>("RconServerStatus");
            operation.Telemetry.Type = $"{gameServerDto.GameType}Server";
            operation.Telemetry.Target = $"{gameServerDto.Hostname}:{gameServerDto.QueryPort}";

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
                            Rate = p.Rate,
                            Ping = p.Ping
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
