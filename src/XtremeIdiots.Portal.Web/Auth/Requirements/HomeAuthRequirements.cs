using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing home page functionality
/// </summary>
public class AccessHome : IAuthorizationRequirement
{
}