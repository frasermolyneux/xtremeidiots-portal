using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;

namespace XI.Portal.Auth.Extensions
{
    public static class XtremeIdiotsAuthorizationExtensions
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
                                 claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.Moderator
                    )
                )
            );
        }
    }
}