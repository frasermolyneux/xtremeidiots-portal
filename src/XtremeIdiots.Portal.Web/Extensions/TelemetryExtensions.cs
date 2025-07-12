using Microsoft.ApplicationInsights.DataContracts;

using System.Security.Claims;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.BanFileMonitors;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.Extensions
{
    public static class TelemetryExtensions
    {
        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, ClaimsPrincipal claimsPrincipal)
        {
            eventTelemetry.Properties.TryAdd("LoggedInAdminId", claimsPrincipal.XtremeIdiotsId());
            eventTelemetry.Properties.TryAdd("LoggedInUsername", claimsPrincipal.Username());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, AdminActionDto adminActionDto)
        {
            eventTelemetry.Properties.TryAdd("PlayerId", adminActionDto.PlayerId.ToString());
            eventTelemetry.Properties.TryAdd("AdminActionType", adminActionDto.Type.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, CreateAdminActionDto createAdminActionDto)
        {
            eventTelemetry.Properties.TryAdd("PlayerId", createAdminActionDto.PlayerId.ToString());
            eventTelemetry.Properties.TryAdd("AdminActionType", createAdminActionDto.Type.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, EditAdminActionDto editAdminActionDto)
        {
            eventTelemetry.Properties.TryAdd("AdminActionId", editAdminActionDto.AdminActionId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, PlayerDto playerDto)
        {
            eventTelemetry.Properties.TryAdd("PlayerId", playerDto.PlayerId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, GameServerDto gameServerDto)
        {
            eventTelemetry.Properties.TryAdd("GameServerId", gameServerDto.GameServerId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, BanFileMonitorDto banFileMonitorDto)
        {
            eventTelemetry.Properties.TryAdd("BanFileMonitorId", banFileMonitorDto.BanFileMonitorId.ToString());
            eventTelemetry.Properties.TryAdd("GameServerId", banFileMonitorDto.GameServerId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, CreateBanFileMonitorDto createBanFileMonitorDto)
        {
            eventTelemetry.Properties.TryAdd("GameServerId", createBanFileMonitorDto.GameServerId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, EditBanFileMonitorDto editBanFileMonitorDto)
        {
            eventTelemetry.Properties.TryAdd("GameServerId", editBanFileMonitorDto.BanFileMonitorId.ToString());

            return eventTelemetry;
        }
    }
}
