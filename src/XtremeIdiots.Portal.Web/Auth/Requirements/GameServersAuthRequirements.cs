using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements;

/// <summary>
/// Authorization requirement for accessing the game servers management interface
/// </summary>
public class AccessGameServers : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for creating new game servers
/// </summary>
public class CreateGameServer : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for deleting existing game servers
/// </summary>
public class DeleteGameServer : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for editing game server settings
/// </summary>
public class EditGameServer : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for editing game server FTP configuration
/// </summary>
public class EditGameServerFtp : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for editing game server RCON configuration
/// </summary>
public class EditGameServerRcon : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing FTP credentials for game servers
/// </summary>
public class ViewFtpCredential : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing game server details
/// </summary>
public class ViewGameServer : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for viewing RCON credentials for game servers
/// </summary>
public class ViewRconCredential : IAuthorizationRequirement
{
}