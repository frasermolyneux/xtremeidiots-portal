using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
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
