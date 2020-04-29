﻿using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.BanFileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;

namespace XI.Portal.Auth.Extensions
{
    public static class PolicyExtensions
    {
        public static void AddXtremeIdiotsPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(XtremeIdiotsPolicy.RootPolicy, policy =>
                policy.RequireClaim(XtremeIdiotsClaimTypes.SeniorAdmin)
            );

            options.AddPolicy(XtremeIdiotsPolicy.ServersManagement, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin || claim.Type == XtremeIdiotsClaimTypes.HeadAdmin
                )));

            options.AddPolicy(XtremeIdiotsPolicy.PlayersManagement, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            options.AddPolicy(XtremeIdiotsPolicy.UserHasCredentials, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                        claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                                 claim.Type == PortalClaimTypes.FtpCredentials ||
                                 claim.Type == PortalClaimTypes.RconCredentials
                    )
                )
            );

            options.AddPolicy(XtremeIdiotsPolicy.ViewServiceStatus, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                        claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                    )
                )
            );

            options.AddPolicy(XtremeIdiotsPolicy.CanAccessServerAdmin, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            options.AddPolicy(XtremeIdiotsPolicy.CanAccessGlobalChatLog, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                )));

            options.AddPolicy(XtremeIdiotsPolicy.CanAccessGameChatLog, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                )));

            options.AddPolicy(XtremeIdiotsPolicy.CanAccessLiveRcon, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            // Admin Actions
            options.AddPolicy(XtremeIdiotsPolicy.AccessAdminActionsController, policy =>
            {
                policy.Requirements.Add(new AccessAdminActionsController());
            });

            options.AddPolicy(XtremeIdiotsPolicy.ChangeAdminActionAdmin, policy =>
            {
                policy.Requirements.Add(new ChangeAdminActionAdmin());
            });

            options.AddPolicy(XtremeIdiotsPolicy.ClaimAdminAction, policy =>
            {
                policy.Requirements.Add(new ClaimAdminAction());
            });

            options.AddPolicy(XtremeIdiotsPolicy.CreateAdminAction, policy =>
            {
                policy.Requirements.Add(new CreateAdminAction());
            });

            options.AddPolicy(XtremeIdiotsPolicy.CreateAdminActionTopic, policy =>
            {
                policy.Requirements.Add(new CreateAdminActionTopic());
            });

            options.AddPolicy(XtremeIdiotsPolicy.DeleteAdminAction, policy =>
            {
                policy.Requirements.Add(new DeleteAdminAction());
            });

            options.AddPolicy(XtremeIdiotsPolicy.EditAdminAction, policy =>
            {
                policy.Requirements.Add(new EditAdminAction());
            });

            options.AddPolicy(XtremeIdiotsPolicy.LiftAdminAction, policy =>
            {
                policy.Requirements.Add(new LiftAdminAction());
            });

            // Ban File Monitors
            options.AddPolicy(XtremeIdiotsPolicy.CreateBanFileMonitor, policy =>
            {
                policy.Requirements.Add(new CreateBanFileMonitor());
            });

            options.AddPolicy(XtremeIdiotsPolicy.ViewBanFileMonitor, policy =>
            {
                policy.Requirements.Add(new ViewBanFileMonitor());
            });

            options.AddPolicy(XtremeIdiotsPolicy.EditBanFileMonitor, policy =>
            {
                policy.Requirements.Add(new EditBanFileMonitor());
            });

            options.AddPolicy(XtremeIdiotsPolicy.DeleteBanFileMonitor, policy =>
            {
                policy.Requirements.Add(new DeleteBanFileMonitor());
            });
        }
    }
}