using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{

    public static class BaseAuthorizationHelper
    {
        #region Constants

        public static class ClaimGroups
        {
            public static readonly string[] SeniorAdminOnly = { UserProfileClaimType.SeniorAdmin };

            public static readonly string[] AllAdminLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.Moderator
            };

            public static readonly string[] BanFileMonitorLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.BanFileMonitor
            };

            public static readonly string[] CredentialsAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.RconCredentials,
                UserProfileClaimType.FtpCredentials
            };

            public static readonly string[] GameServerAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameServer
            };

            public static readonly string[] AdminLevelsExcludingModerators =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin
            };

            public static readonly string[] ServerAdminAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.Moderator,
                UserProfileClaimType.ServerAdmin
            };

            public static readonly string[] LiveRconAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.LiveRcon
            };

            public static readonly string[] SeniorAndHeadAdminOnly =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin
            };

            public static readonly string[] StatusAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.BanFileMonitor
            };
        }

        #endregion

        #region Core Authorization Checks

        public static void CheckSeniorAdminAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
            }
        }

        public static void CheckClaimTypes(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, string[] claimTypes)
        {
            if (context.User.Claims.Any(claim => claimTypes.Contains(claim.Type)))
            {
                context.Succeed(requirement);
            }
        }

        public static void CheckHeadAdminAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        public static void CheckGameAdminAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            var gameTypeString = gameType.ToString();

            if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameTypeString) ||
                context.User.HasClaim(UserProfileClaimType.GameAdmin, gameTypeString))
            {
                context.Succeed(requirement);
            }
        }

        public static void CheckModeratorAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            var gameTypeString = gameType.ToString();

            if (context.User.HasClaim(UserProfileClaimType.Moderator, gameTypeString) ||
                context.User.HasClaim(UserProfileClaimType.GameAdmin, gameTypeString))
            {
                context.Succeed(requirement);
            }
        }

        #endregion

        #region Composite Authorization Checks

        public static void CheckSeniorOrGameAdminAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {

            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            if (context.Resource is GameType gameType)
            {
                CheckGameAdminAccess(context, requirement, gameType);
            }
        }

        public static void CheckSeniorOrHeadAdminAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {

            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            if (context.Resource is GameType gameType)
            {
                CheckHeadAdminAccess(context, requirement, gameType);
            }
        }

        public static void CheckSeniorOrGameTypeServerAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {

            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            if (context.Resource is Tuple<GameType, Guid> resource)
            {
                var (gameType, gameServerId) = resource;
                CheckGameTypeAndServerAccess(context, requirement, gameType, gameServerId);
            }
        }

        public static void CheckSeniorOrMultipleGameAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {

            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            if (context.Resource is GameType gameType)
            {
                CheckHeadAdminAccess(context, requirement, gameType);
                CheckGameAdminAccess(context, requirement, gameType);
                CheckModeratorAccess(context, requirement, gameType);
            }
        }

        public static void CheckSeniorOrLiveRconAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {

            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            if (context.Resource is GameType gameType)
            {
                CheckGameAdminAccess(context, requirement, gameType);
                CheckLiveRconAccess(context, requirement, gameType);
            }
        }

        #endregion

        #region Server-Specific Authorization

        public static void CheckBanFileMonitorAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
        {
            if (context.User.HasClaim(UserProfileClaimType.BanFileMonitor, gameServerId.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        public static void CheckGameTypeAndServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType, Guid gameServerId)
        {
            CheckHeadAdminAccess(context, requirement, gameType);
            CheckBanFileMonitorAccess(context, requirement, gameServerId);
        }

        #endregion

        #region Game Server Authorization

        public static void CheckGameServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            if (context.User.HasClaim(UserProfileClaimType.GameServer, gameType.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        public static void CheckRconCredentialsAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
        {
            if (context.User.HasClaim(UserProfileClaimType.RconCredentials, gameServerId.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        public static void CheckFtpCredentialsAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
        {
            if (context.User.HasClaim(UserProfileClaimType.FtpCredentials, gameServerId.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        public static void CheckCombinedGameServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            CheckHeadAdminAccess(context, requirement, gameType);
            CheckGameServerAccess(context, requirement, gameType);
        }

        public static void CheckLiveRconAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            if (context.User.HasClaim(UserProfileClaimType.LiveRcon, gameType.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        #endregion

        #region Utility Methods

        public static string? GetUserXtremeIdiotsId(AuthorizationHandlerContext context)
        {
            return context.User.FindFirst(UserProfileClaimType.XtremeIdiotsId)?.Value;
        }

        public static bool IsActionOwner(AuthorizationHandlerContext context, string? adminId)
        {
            var userXtremeId = GetUserXtremeIdiotsId(context);
            return userXtremeId == adminId;
        }

        public static string? GetUserProfileId(AuthorizationHandlerContext context)
        {
            return context.User.FindFirst(UserProfileClaimType.UserProfileId)?.Value;
        }

        public static bool IsResourceOwner(AuthorizationHandlerContext context, Guid resourceUserProfileId)
        {
            var userProfileId = GetUserProfileId(context);
            return userProfileId != null && userProfileId == resourceUserProfileId.ToString();
        }

        public static void CheckAuthenticated(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
            }
        }

        #endregion
    }
}