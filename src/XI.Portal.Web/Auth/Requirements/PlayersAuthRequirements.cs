using Microsoft.AspNetCore.Authorization;

namespace XI.Portal.Web.Auth.Requirements
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
