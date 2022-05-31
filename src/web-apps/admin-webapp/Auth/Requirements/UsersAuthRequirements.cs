using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Requirements
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
