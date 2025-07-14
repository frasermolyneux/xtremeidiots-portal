using Microsoft.ApplicationInsights.DataContracts;

using System.Security.Claims;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.BanFileMonitors;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Demos;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;

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
            eventTelemetry.Properties.TryAdd("BanFileMonitorId", editBanFileMonitorDto.BanFileMonitorId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, DemoDto demoDto)
        {
            eventTelemetry.Properties.TryAdd("DemoId", demoDto.DemoId.ToString());
            eventTelemetry.Properties.TryAdd("GameType", demoDto.GameType.ToString());
            eventTelemetry.Properties.TryAdd("DemoTitle", demoDto.Title ?? "Unknown");

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, MapPackDto mapPackDto)
        {
            eventTelemetry.Properties.TryAdd("MapPackId", mapPackDto.MapPackId.ToString());
            eventTelemetry.Properties.TryAdd("GameServerId", mapPackDto.GameServerId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, CreateMapPackDto createMapPackDto)
        {
            eventTelemetry.Properties.TryAdd("GameServerId", createMapPackDto.GameServerId.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, TagDto tagDto)
        {
            eventTelemetry.Properties.TryAdd("TagId", tagDto.TagId.ToString());
            eventTelemetry.Properties.TryAdd("TagName", tagDto.Name);
            eventTelemetry.Properties.TryAdd("UserDefined", tagDto.UserDefined.ToString());

            return eventTelemetry;
        }

        public static EventTelemetry Enrich(this EventTelemetry eventTelemetry, UserProfileDto userProfileDto)
        {
            eventTelemetry.Properties.TryAdd("UserProfileId", userProfileDto.UserProfileId.ToString());
            eventTelemetry.Properties.TryAdd("DisplayName", userProfileDto.DisplayName ?? "Unknown");

            return eventTelemetry;
        }

        // ExceptionTelemetry enrichment methods
        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, ClaimsPrincipal claimsPrincipal)
        {
            exceptionTelemetry.Properties.TryAdd("LoggedInAdminId", claimsPrincipal.XtremeIdiotsId());
            exceptionTelemetry.Properties.TryAdd("LoggedInUsername", claimsPrincipal.Username());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, AdminActionDto adminActionDto)
        {
            exceptionTelemetry.Properties.TryAdd("PlayerId", adminActionDto.PlayerId.ToString());
            exceptionTelemetry.Properties.TryAdd("AdminActionId", adminActionDto.AdminActionId.ToString());
            exceptionTelemetry.Properties.TryAdd("AdminActionType", adminActionDto.Type.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, CreateAdminActionDto createAdminActionDto)
        {
            exceptionTelemetry.Properties.TryAdd("PlayerId", createAdminActionDto.PlayerId.ToString());
            exceptionTelemetry.Properties.TryAdd("AdminActionType", createAdminActionDto.Type.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, EditAdminActionDto editAdminActionDto)
        {
            exceptionTelemetry.Properties.TryAdd("AdminActionId", editAdminActionDto.AdminActionId.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, PlayerDto playerDto)
        {
            exceptionTelemetry.Properties.TryAdd("PlayerId", playerDto.PlayerId.ToString());
            exceptionTelemetry.Properties.TryAdd("GameType", playerDto.GameType.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, GameServerDto gameServerDto)
        {
            exceptionTelemetry.Properties.TryAdd("GameServerId", gameServerDto.GameServerId.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, BanFileMonitorDto banFileMonitorDto)
        {
            exceptionTelemetry.Properties.TryAdd("BanFileMonitorId", banFileMonitorDto.BanFileMonitorId.ToString());
            exceptionTelemetry.Properties.TryAdd("GameServerId", banFileMonitorDto.GameServerId.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, CreateBanFileMonitorDto createBanFileMonitorDto)
        {
            exceptionTelemetry.Properties.TryAdd("GameServerId", createBanFileMonitorDto.GameServerId.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, EditBanFileMonitorDto editBanFileMonitorDto)
        {
            exceptionTelemetry.Properties.TryAdd("BanFileMonitorId", editBanFileMonitorDto.BanFileMonitorId.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, DemoDto demoDto)
        {
            exceptionTelemetry.Properties.TryAdd("DemoId", demoDto.DemoId.ToString());
            exceptionTelemetry.Properties.TryAdd("GameType", demoDto.GameType.ToString());
            exceptionTelemetry.Properties.TryAdd("DemoTitle", demoDto.Title ?? "Unknown");

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, MapPackDto mapPackDto)
        {
            exceptionTelemetry.Properties.TryAdd("MapPackId", mapPackDto.MapPackId.ToString());
            exceptionTelemetry.Properties.TryAdd("GameServerId", mapPackDto.GameServerId.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, CreateMapPackDto createMapPackDto)
        {
            exceptionTelemetry.Properties.TryAdd("GameServerId", createMapPackDto.GameServerId.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, TagDto tagDto)
        {
            exceptionTelemetry.Properties.TryAdd("TagId", tagDto.TagId.ToString());
            exceptionTelemetry.Properties.TryAdd("TagName", tagDto.Name);
            exceptionTelemetry.Properties.TryAdd("UserDefined", tagDto.UserDefined.ToString());

            return exceptionTelemetry;
        }

        public static ExceptionTelemetry Enrich(this ExceptionTelemetry exceptionTelemetry, UserProfileDto userProfileDto)
        {
            exceptionTelemetry.Properties.TryAdd("UserProfileId", userProfileDto.UserProfileId.ToString());
            exceptionTelemetry.Properties.TryAdd("DisplayName", userProfileDto.DisplayName ?? "Unknown");

            return exceptionTelemetry;
        }

    }
}
