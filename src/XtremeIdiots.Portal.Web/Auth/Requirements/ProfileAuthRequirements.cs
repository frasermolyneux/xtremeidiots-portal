using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing profile functionality
/// </summary>
public class AccessProfile : IAuthorizationRequirement
{
}