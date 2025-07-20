using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing user management functionality
/// </summary>
public class AccessUsers : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for creating user claims
/// </summary>
public class CreateUserClaim : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting user claims
/// </summary>
public class DeleteUserClaim : IAuthorizationRequirement
{
}