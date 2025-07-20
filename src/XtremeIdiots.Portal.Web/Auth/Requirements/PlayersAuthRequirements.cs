using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing players functionality
/// </summary>
public class AccessPlayers : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting players
/// </summary>
public class DeletePlayer : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing player information
/// </summary>
public class ViewPlayers : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for creating protected names
/// </summary>
public class CreateProtectedName : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting protected names
/// </summary>
public class DeleteProtectedName : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing protected names
/// </summary>
public class ViewProtectedName : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for accessing player tags functionality
/// </summary>
public class AccessPlayerTags : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for creating player tags
/// </summary>
public class CreatePlayerTag : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for editing player tags
/// </summary>
public class EditPlayerTag : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting player tags
/// </summary>
public class DeletePlayerTag : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing player tags
/// </summary>
public class ViewPlayerTag : IAuthorizationRequirement
{
}