using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing system status and monitoring functionality
/// </summary>
public class AccessStatus : IAuthorizationRequirement
{
}