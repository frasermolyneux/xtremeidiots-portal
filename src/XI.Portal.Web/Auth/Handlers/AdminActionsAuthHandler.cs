using Microsoft.AspNetCore.Authorization;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Auth.Requirements;
using XI.Portal.Web.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Web.Auth.Handlers
{
    public class AdminActionsAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessAdminActions)
                    HandleAccessAdminActions(context, requirement);

                if (requirement is ChangeAdminActionAdmin)
                    HandleChangeAdminActionAdmin(context, requirement);

                if (requirement is ClaimAdminAction)
                    HandleClaimAdminAction(context, requirement);

                if (requirement is LiftAdminAction)
                    HandleLiftAdminAction(context, requirement);

                if (requirement is CreateAdminAction)
                    HandleCreateAdminAction(context, requirement);

                if (requirement is EditAdminAction)
                    HandleEditAdminAction(context, requirement);

                if (requirement is DeleteAdminAction)
                    HandleDeleteAdminAction(context, requirement);

                if (requirement is CreateAdminActionTopic)
                    HandleCreateAdminActionTopic(context, requirement);
            }

            return Task.CompletedTask;
        }

        private static void HandleCreateAdminActionTopic(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleDeleteAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);
        }

        private static void HandleEditAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, AdminActionType, string>)
            {
                var (gameType, adminActionType, adminId) = (Tuple<GameType, AdminActionType, string>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                switch (adminActionType)
                {
                    case AdminActionType.Observation:
                        if ((context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, gameType.ToString()) ||
                             context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString())) &&
                            context.User.XtremeIdiotsId() == adminId)
                            context.Succeed(requirement);
                        break;
                    case AdminActionType.Warning:
                        if ((context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, gameType.ToString()) ||
                             context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString())) &&
                            context.User.XtremeIdiotsId() == adminId)
                            context.Succeed(requirement);
                        break;
                    case AdminActionType.Kick:
                        if ((context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, gameType.ToString()) ||
                             context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString())) &&
                            context.User.XtremeIdiotsId() == adminId)
                            context.Succeed(requirement);
                        break;
                    case AdminActionType.TempBan:
                        if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString()) &&
                            context.User.XtremeIdiotsId() == adminId)
                            context.Succeed(requirement);
                        break;
                    case AdminActionType.Ban:
                        if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString()) &&
                            context.User.XtremeIdiotsId() == adminId)
                            context.Succeed(requirement);
                        break;
                }
            }
        }

        private static void HandleCreateAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, AdminActionType>)
            {
                var (gameType, adminActionType) = (Tuple<GameType, AdminActionType>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                switch (adminActionType)
                {
                    case AdminActionType.Observation:
                        if (context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, gameType.ToString()))
                            context.Succeed(requirement);
                        break;
                    case AdminActionType.Warning:
                        if (context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, gameType.ToString()))
                            context.Succeed(requirement);
                        break;
                    case AdminActionType.Kick:
                        if (context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, gameType.ToString()))
                            context.Succeed(requirement);
                        break;
                }
            }
        }

        private static void HandleLiftAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, string>)
            {
                var (gameType, adminId) = (Tuple<GameType, string>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString()) &&
                    context.User.XtremeIdiotsId() == adminId)
                    context.Succeed(requirement);
            }
        }

        private static void HandleClaimAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleChangeAdminActionAdmin(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleAccessAdminActions(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);
        }
    }
}
