using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ApiControllers;

[Authorize(Policy = AuthPolicies.AccessPlayers)]
[Route("PlayerAnalytics")]
public class PlayerAnalyticsController(
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<PlayerAnalyticsController> logger,
    IConfiguration configuration) : BaseApiController(telemetryClient, logger, configuration)
{
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    [HttpGet("GetCumulativeDailyPlayersJson")]
    public async Task<IActionResult> GetCumulativeDailyPlayersJson(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            Logger.LogInformation("User {UserId} requesting cumulative daily players data from {Cutoff}",
                User.XtremeIdiotsId(), cutoff);

            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetCumulativeDailyPlayers(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data is null)
            {
                Logger.LogWarning("Failed to retrieve cumulative daily players data for user {UserId}", User.XtremeIdiotsId());
                return StatusCode(500, "Failed to retrieve cumulative daily players data");
            }

            Logger.LogInformation("Successfully retrieved cumulative daily players data for user {UserId}", User.XtremeIdiotsId());
            return Ok(playerAnalyticsResponse.Result.Data);
        }, "GetCumulativeDailyPlayersJson", $"cutoff: {cutoff}");
    }

    [HttpGet("GetNewDailyPlayersPerGameJson")]
    public async Task<IActionResult> GetNewDailyPlayersPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            Logger.LogInformation("User {UserId} requesting new daily players per game data from {Cutoff}",
                User.XtremeIdiotsId(), cutoff);

            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetNewDailyPlayersPerGame(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data is null)
            {
                Logger.LogWarning("Failed to retrieve new daily players per game data for user {UserId}", User.XtremeIdiotsId());
                return StatusCode(500, "Failed to retrieve new daily players per game data");
            }

            Logger.LogInformation("Successfully retrieved new daily players per game data for user {UserId}", User.XtremeIdiotsId());
            return Ok(playerAnalyticsResponse.Result.Data);
        }, "GetNewDailyPlayersPerGameJson", $"cutoff: {cutoff}");
    }

    [HttpGet("GetPlayersDropOffPerGameJson")]
    public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            Logger.LogInformation("User {UserId} requesting players drop-off per game data from {Cutoff}",
                User.XtremeIdiotsId(), cutoff);

            var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetPlayersDropOffPerGameJson(cutoff);

            if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data is null)
            {
                Logger.LogWarning("Failed to retrieve players drop-off per game data for user {UserId}", User.XtremeIdiotsId());
                return StatusCode(500, "Failed to retrieve players drop-off per game data");
            }

            Logger.LogInformation("Successfully retrieved players drop-off per game data for user {UserId}", User.XtremeIdiotsId());
            return Ok(playerAnalyticsResponse.Result.Data);
        }, "GetPlayersDropOffPerGameJson", $"cutoff: {cutoff}");
    }
}