using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing demo functionality
/// </summary>
/// <remarks>
/// Controls access to demo listing, searching, and viewing operations
/// </remarks>
public class AccessDemos : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting demo files
/// </summary>
/// <remarks>
/// Controls access to demo deletion operations, typically restricted to administrators
/// </remarks>
public class DeleteDemo : IAuthorizationRequirement
{
}