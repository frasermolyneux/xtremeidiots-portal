using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{

    public class AccessUsers : IAuthorizationRequirement
    {
    }

    public class CreateUserClaim : IAuthorizationRequirement
    {
    }

    public class DeleteUserClaim : IAuthorizationRequirement
    {
    }
}