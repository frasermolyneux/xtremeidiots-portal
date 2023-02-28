using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Requirements
{
    public class AccessPlayers : IAuthorizationRequirement
    {
    }

    public class DeletePlayer : IAuthorizationRequirement
    {
    }

    public class ViewPlayers : IAuthorizationRequirement
    {
    }
}
