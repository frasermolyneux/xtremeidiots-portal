using System.Linq;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;

namespace XI.Portal.Auth.XtremeIdiots
{
    public static class XtremeIdiotsAuthorizationExtensions
    {
        private static readonly string[] SeniorAdminGroups = {"Senior Admin"};

        private static readonly string[] HeadAdminGroups =
        {
            "COD2 Head Admin", "COD4 Head Admin", "COD5 Head Admin", "Rust Head Admin", "Insurgency Head Admin",
            "ARMA Head Admin", "Minecraft Head Admin", "Battlefield Head Admin"
        };

        private static readonly string[] AdminGroups =
        {
            "COD2 Admin", "COD4 Admin", "COD5 Admin", "Rust Admin", "Insurgency Admin",
            "ARMA Admin", "Minecraft Admin", "Battlefield Admin"
        };

        public static void AddXtremeIdiotsPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(XtremeIdiotsPolicy.SeniorAdmin, policy =>
                policy.RequireClaim(XtremeIdiotsClaimTypes.Group, SeniorAdminGroups)
            );

            options.AddPolicy(XtremeIdiotsPolicy.HeadAdmin, policy =>
                policy.RequireClaim(XtremeIdiotsClaimTypes.Group, SeniorAdminGroups.Concat(HeadAdminGroups))
            );

            options.AddPolicy(XtremeIdiotsPolicy.HeadAdminX, policy =>
                policy.RequireClaim(XtremeIdiotsClaimTypes.Group, HeadAdminGroups)
            );

            options.AddPolicy(XtremeIdiotsPolicy.Admin, policy =>
                policy.RequireClaim(XtremeIdiotsClaimTypes.Group,
                    SeniorAdminGroups.Concat(HeadAdminGroups).Concat(AdminGroups))
            );

            options.AddPolicy(XtremeIdiotsPolicy.AdminX, policy =>
                policy.RequireClaim(XtremeIdiotsClaimTypes.Group, AdminGroups)
            );

            options.AddPolicy(XtremeIdiotsPolicy.Management, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(claim =>
                    claim.Type == XtremeIdiotsClaimTypes.Group &&
                    (SeniorAdminGroups.Contains(claim.Value) || HeadAdminGroups.Contains(claim.Value))))
            );
        }
    }
}