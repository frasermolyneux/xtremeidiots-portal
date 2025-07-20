using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing live RCON functionality
/// </summary>
public class AccessLiveRcon : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for accessing server admin functionality
/// </summary>
public class AccessServerAdmin : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing game chat logs
/// </summary>
public class ViewGameChatLog : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing global chat logs
/// </summary>
public class ViewGlobalChatLog : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing live RCON interface
/// </summary>
public class ViewLiveRcon : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing server-specific chat logs
/// </summary>
public class ViewServerChatLog : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for locking chat messages
/// </summary>
public class LockChatMessages : IAuthorizationRequirement
{
}