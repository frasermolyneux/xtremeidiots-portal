using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ApiControllers;

[Route("External")]
public class ExternalController : BaseApiController
{
    private readonly IRepositoryApiClient repositoryApiClient;

    public ExternalController(
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<ExternalController> logger,
    IConfiguration configuration)
    : base(telemetryClient, logger, configuration)
    {
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    }

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
                string actionText;
                if (adminActionDto.Expires <= DateTime.UtcNow &&
         (adminActionDto.Type == AdminActionType.Ban || adminActionDto.Type == AdminActionType.TempBan))
                    actionText = $"lifted a {adminActionDto.Type} on";
                else
                    actionText = $"added a {adminActionDto.Type} to";

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