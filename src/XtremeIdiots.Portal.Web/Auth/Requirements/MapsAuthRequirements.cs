using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the maps display functionality.
    /// Allows users to view the maps index page and browse available Call of Duty
    /// maps without management capabilities.
    /// </summary>
    public class AccessMaps : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for accessing the map manager controller.
    /// Provides access to the map management interface for game servers,
    /// including viewing server maps and available map packs.
    /// </summary>
    public class AccessMapManagerController : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for managing maps on game servers.
    /// Allows users to perform map management operations including viewing
    /// map packs, managing server map configurations, and coordinating
    /// map deployments. Authorization is scoped to specific game types.
    /// </summary>
    public class ManageMaps : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for creating new map packs.
    /// Allows users to create collections of maps that can be deployed
    /// to Call of Duty game servers. Map packs organize related maps
    /// for easier server configuration and deployment.
    /// </summary>
    public class CreateMapPack : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for editing existing map packs.
    /// Allows modification of map pack contents, descriptions, and
    /// associated game server configurations.
    /// </summary>
    public class EditMapPack : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting map packs.
    /// Allows permanent removal of map pack configurations from the system.
    /// This operation affects map deployment capabilities for associated servers.
    /// </summary>
    public class DeleteMapPack : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for pushing maps to remote game servers.
    /// Allows users to deploy map files from map packs to Call of Duty
    /// game servers via FTP or other remote deployment mechanisms.
    /// This is a high-privilege operation requiring server access credentials.
    /// </summary>
    public class PushMapToRemote : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting maps from remote game server hosts.
    /// Allows users to remove map files directly from Call of Duty game servers.
    /// This is a high-privilege operation that can affect server functionality
    /// and requires server access credentials.
    /// </summary>
    public class DeleteMapFromHost : IAuthorizationRequirement
    {
    }
}
