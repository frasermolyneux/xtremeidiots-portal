using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing maps functionality
/// </summary>
public class AccessMaps : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for accessing the map manager controller
/// </summary>
public class AccessMapManagerController : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for managing maps
/// </summary>
public class ManageMaps : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for creating map packs
/// </summary>
public class CreateMapPack : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for editing map packs
/// </summary>
public class EditMapPack : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting map packs
/// </summary>
public class DeleteMapPack : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for pushing maps to remote servers
/// </summary>
public class PushMapToRemote : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting maps from host servers
/// </summary>
public class DeleteMapFromHost : IAuthorizationRequirement
{
}