using Microsoft.ApplicationInsights.DataContracts;

using System.Security.Claims;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.Extensions
{
    public static class TelemetryExtensions
    {
        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, ClaimsPrincipal claimsPrincipal)
        {
            eventTelemetry.Properties.Add("LoggedInAdminId", claimsPrincipal.XtremeIdiotsId());
            eventTelemetry.Properties.Add("LoggedInUsername", claimsPrincipal.Username());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, AdminActionDto adminActionDto)
        {
            eventTelemetry.Properties.Add("PlayerId", adminActionDto.PlayerId.ToString());
            eventTelemetry.Properties.Add("AdminActionType", adminActionDto.Type.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, CreateAdminActionDto createAdminActionDto)
        {
            eventTelemetry.Properties.Add("PlayerId", createAdminActionDto.PlayerId.ToString());
            eventTelemetry.Properties.Add("AdminActionType", createAdminActionDto.Type.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, EditAdminActionDto editAdminActionDto)
        {
            eventTelemetry.Properties.Add("AdminActionId", editAdminActionDto.AdminActionId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, PlayerDto playerDto)
        {
            eventTelemetry.Properties.Add("PlayerId", playerDto.Id.ToString());

            return eventTelemetry;
        }
    }
}
