using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ApiControllers;

/// <summary>
/// API controller providing external endpoints for integration with forum widgets and other external services
/// </summary>
/// <remarks>
/// Initializes a new instance of the ExternalController
/// </remarks>
/// <param name="repositoryApiClient">Client for accessing the repository API</param>
/// <param name="telemetryClient">Application Insights telemetry client</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
[Route("External")]
public class ExternalController(
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<ExternalController> logger,
    IConfiguration configuration) : BaseApiController(telemetryClient, logger, configuration)
{
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Retrieves the latest admin actions for display in external forum widgets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>JSON array of recent admin actions with formatted display data</returns>
    [HttpGet("GetLatestAdminActions")]
    [EnableCors("CorsPolicy")]
    public async Task<IActionResult> GetLatestAdminActions(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            Logger.LogInformation("External API request for latest admin actions JSON data");

            var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(
                null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve admin actions for external API - API response unsuccessful or data is null");
                return StatusCode(500, "Failed to retrieve admin actions data");
            }

            var results = new List<dynamic>();
            foreach (var adminActionDto in adminActionsApiResponse.Result.Data.Items)
            {
                // Determine action text based on admin action type and expiration status
                var actionText = adminActionDto.Expires <= DateTime.UtcNow &&
                    (adminActionDto.Type == AdminActionType.Ban || adminActionDto.Type == AdminActionType.TempBan)
                    ? $"lifted a {adminActionDto.Type} on"
                    : $"added a {adminActionDto.Type} to";
                var adminName = adminActionDto.UserProfile?.DisplayName ?? "Unknown";
                var adminId = adminActionDto.UserProfile?.XtremeIdiotsForumId;

                results.Add(new
                {
                    GameIconUrl = $"https://portal.xtremeidiots.com/images/game-icons/{adminActionDto.Player?.GameType.ToString()}.png",
                    AdminName = adminName,
                    AdminId = adminId,
                    ActionType = adminActionDto.Type.ToString(),
                    ActionText = actionText,
                    PlayerName = adminActionDto.Player?.Username,
                    PlayerLink = $"https://portal.xtremeidiots.com/Players/Details/{adminActionDto.PlayerId}"
                });
            }

            Logger.LogInformation("Successfully processed {Count} admin actions for external API response", results.Count);

            TrackSuccessTelemetry(nameof(GetLatestAdminActions), nameof(GetLatestAdminActions), new Dictionary<string, string>
            {
                { nameof(ExternalController), nameof(ExternalController) },
                { "Resource", "AdminActionsAPI" },
                { "Count", results.Count.ToString() }
            });

            return Ok(results);
        }, nameof(GetLatestAdminActions));
    }
}