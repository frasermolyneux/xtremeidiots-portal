using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the game servers functionality.
    /// Allows users to view and browse game servers but not modify them.
    /// Game servers represent Call of Duty dedicated servers managed by the community.
    /// </summary>
    public class AccessGameServers : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for creating new game servers.
    /// Allows users to register new Call of Duty dedicated servers in the system,
    /// including setting up basic server configuration and connection details.
    /// Authorization is scoped to specific game types (COD2, COD4, COD5, etc.).
    /// </summary>
    public class CreateGameServer : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting game servers.
    /// Allows permanent removal of game server configurations from the system.
    /// This is a high-privilege operation typically restricted to senior administrators.
    /// </summary>
    public class DeleteGameServer : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for editing basic game server properties.
    /// Allows modification of server details like title, hostname, and query port,
    /// but excludes sensitive credential information (FTP/RCON) which require 
    /// separate authorization requirements.
    /// </summary>
    public class EditGameServer : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for editing game server FTP credentials.
    /// Allows modification of FTP connection details including hostname, port,
    /// username, and password. This is a high-privilege operation as FTP access
    /// enables file management on the game servers.
    /// </summary>
    public class EditGameServerFtp : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for editing game server RCON credentials.
    /// Allows modification of RCON (Remote Console) connection details.
    /// This is a high-privilege operation as RCON enables real-time server
    /// administration and player management.
    /// </summary>
    public class EditGameServerRcon : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing FTP credential information.
    /// Allows users to see FTP connection details for servers they manage.
    /// Used in conjunction with AccessCredentials for the credentials page.
    /// Authorization is scoped to specific game types and server instances.
    /// </summary>
    public class ViewFtpCredential : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing detailed game server information.
    /// Allows access to server details, statistics, and configuration information
    /// but not modification capabilities.
    /// </summary>
    public class ViewGameServer : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing RCON credential information.
    /// Allows users to see RCON connection details for servers they manage.
    /// Used in conjunction with AccessCredentials for the credentials page.
    /// Authorization is scoped to specific game types and server instances.
    /// </summary>
    public class ViewRconCredential : IAuthorizationRequirement
    {
    }
}
