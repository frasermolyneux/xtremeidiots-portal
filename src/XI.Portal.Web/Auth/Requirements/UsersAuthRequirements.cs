using Microsoft.AspNetCore.Authorization;

namespace XI.Portal.Web.Auth.Requirements
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
