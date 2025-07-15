using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing live RCON functionality across the gaming platform.
    /// This is a high-level permission that grants access to real-time Remote Console operations
    /// for Call of Duty game servers, allowing users to execute server commands in real-time.
    /// </summary>
    /// <remarks>
    /// Live RCON access enables server administrators to perform real-time server management
    /// operations such as kicking players, restarting maps, and executing custom server commands.
    /// This permission is typically restricted to trusted server administrators and moderators.
    /// Authorization handlers should verify user has appropriate game-type specific permissions.
    /// </remarks>
    public class AccessLiveRcon : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for accessing the server administration dashboard and tools.
    /// This is the primary entry point permission for all server administrative functionality
    /// in the XtremeIdiots gaming community management portal.
    /// </summary>
    /// <remarks>
    /// Server admin access provides the foundation for managing Call of Duty game servers,
    /// including viewing server status, accessing RCON interfaces, managing chat logs,
    /// and performing administrative actions. This permission serves as the base requirement
    /// for all server management operations and should be combined with more specific
    /// permissions for granular access control.
    /// </remarks>
    public class AccessServerAdmin : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing chat logs filtered by specific game types.
    /// Allows users to access chat message history for Call of Duty games they have
    /// administrative permissions for (COD2, COD4, COD5).
    /// </summary>
    /// <remarks>
    /// Game chat log access is game-type specific, meaning users can only view chat logs
    /// for games they are authorized to moderate. This supports the community's role-based
    /// administration model where different moderators may be responsible for different
    /// Call of Duty game versions. Chat logs are essential for investigating player
    /// behavior, resolving disputes, and maintaining community standards.
    /// </remarks>
    public class ViewGameChatLog : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing chat logs across all game types and servers.
    /// This is an elevated permission that grants access to the complete chat message
    /// history across the entire XtremeIdiots gaming community platform.
    /// </summary>
    /// <remarks>
    /// Global chat log access is typically reserved for senior administrators and moderators
    /// who need oversight capabilities across all Call of Duty game servers. This permission
    /// enables community management at scale, cross-game moderation, and comprehensive
    /// investigation of community-wide issues. Users with this permission can view chat
    /// messages from all supported game types and all managed servers.
    /// </remarks>
    public class ViewGlobalChatLog : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing live RCON interfaces for specific servers.
    /// This permission controls access to real-time Remote Console viewing capabilities
    /// where users can observe but may have limited interaction with server commands.
    /// </summary>
    /// <remarks>
    /// Live RCON viewing is typically a prerequisite for full RCON access and allows
    /// users to monitor server status, player activity, and server console output in
    /// real-time. This permission is often granted to junior administrators or monitoring
    /// roles who need visibility into server operations without full command execution
    /// capabilities. Authorization should verify game-type and server-specific permissions.
    /// </remarks>
    public class ViewLiveRcon : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing chat logs from specific game servers.
    /// Provides server-level granular access to chat message history for users
    /// responsible for individual server administration.
    /// </summary>
    /// <remarks>
    /// Server chat log access enables focused moderation of individual Call of Duty
    /// game servers within the community. This permission supports distributed
    /// administration where different users may be responsible for specific servers
    /// rather than entire game types. It's essential for server-specific dispute
    /// resolution, player behavior monitoring, and maintaining server-level community
    /// standards within the broader gaming platform.
    /// </remarks>
    public class ViewServerChatLog : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for locking and unlocking chat messages in the system.
    /// This administrative function prevents modification or deletion of important
    /// chat log entries that may be evidence in disciplinary actions or investigations.
    /// </summary>
    /// <remarks>
    /// Chat message locking is a critical moderation tool that preserves evidence
    /// for administrative actions, appeals, and community management decisions.
    /// Locked messages cannot be modified or deleted, ensuring audit trail integrity
    /// for the gaming community. This permission is typically restricted to senior
    /// moderators and administrators who handle disciplinary proceedings and need
    /// to preserve evidence of rule violations or inappropriate behavior.
    /// </remarks>
    public class LockChatMessages : IAuthorizationRequirement
    {
    }
}
