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

    public class AccessPlayerTags : IAuthorizationRequirement
    {
    }

    public class CreatePlayerTag : IAuthorizationRequirement
    {
    }

    public class EditPlayerTag : IAuthorizationRequirement
    {
    }

    public class DeletePlayerTag : IAuthorizationRequirement
    {
    }

    public class ViewPlayerTag : IAuthorizationRequirement
    {
    }
}