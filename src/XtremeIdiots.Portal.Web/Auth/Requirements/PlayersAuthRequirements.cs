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

    public class CreateProtectedName : IAuthorizationRequirement
    {
    }

    public class DeleteProtectedName : IAuthorizationRequirement
    {
    }

    public class ViewProtectedName : IAuthorizationRequirement
    {
    }



    public class ViewPlayerTag : IAuthorizationRequirement
    {
    }
}
