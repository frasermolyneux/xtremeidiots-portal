using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the user management section of the portal.
    /// This permission controls access to view user accounts, manage user profiles,
    /// and access the administrative interface for community member management.
    /// </summary>
    /// <remarks>
    /// The AccessUsers requirement is designed for administrative access to user management
    /// functionality within the XtremeIdiots gaming portal. This includes viewing the list
    /// of registered community members, accessing individual user profiles, managing user
    /// sessions, and overseeing community member accounts.
    /// 
    /// This permission is typically granted to administrators, moderators, and senior
    /// community staff who need to manage user accounts, investigate user behavior,
    /// and maintain community standards. Users with this permission can view user
    /// profiles, manage user sessions (including forced logout), and access user
    /// management dashboards.
    /// 
    /// The authorization handlers should verify that users have appropriate administrative
    /// privileges for community management and user oversight responsibilities.
    /// This permission provides read and management access to user accounts but does
    /// not include the ability to modify user permissions directly.
    /// </remarks>
    public class AccessUsers : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for creating user permission claims within the gaming platform.
    /// This permission allows administrators to grant specific game-type permissions to
    /// community members, enabling role-based access control across Call of Duty servers.
    /// </summary>
    /// <remarks>
    /// The CreateUserClaim requirement enables administrators to assign game-specific
    /// permissions to community members within the XtremeIdiots gaming portal. This includes
    /// granting moderation privileges, server administration rights, and other role-based
    /// permissions for specific Call of Duty game types (COD2, COD4, COD5).
    /// 
    /// This is a critical administrative function that directly impacts the security and
    /// operational structure of the gaming community. Users with this permission can
    /// elevate other users' privileges, assign server-specific roles, and manage the
    /// permission hierarchy across different game servers.
    /// 
    /// Authorization handlers should verify that users have appropriate administrative
    /// authority for the specific game type they are managing. This permission is
    /// typically restricted to senior administrators and is often game-type specific,
    /// meaning administrators can only create claims for game types they have authority over.
    /// The system should audit all claim creation activities for security and accountability.
    /// </remarks>
    public class CreateUserClaim : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for removing user permission claims from community members.
    /// This permission allows administrators to revoke specific game-type permissions,
    /// enabling disciplinary actions and role management across Call of Duty servers.
    /// </summary>
    /// <remarks>
    /// The DeleteUserClaim requirement enables administrators to remove game-specific
    /// permissions from community members within the XtremeIdiots gaming portal. This includes
    /// revoking moderation privileges, removing server administration rights, and managing
    /// disciplinary actions for specific Call of Duty game types (COD2, COD4, COD5).
    /// 
    /// This is a critical administrative function used for disciplinary actions, role
    /// changes, and security management within the gaming community. Users with this
    /// permission can demote other users, remove server-specific access, and manage the
    /// permission hierarchy as part of community governance and security procedures.
    /// 
    /// Authorization handlers should verify that users have appropriate administrative
    /// authority for the specific game type they are managing. This permission is
    /// typically restricted to senior administrators and is often game-type specific,
    /// meaning administrators can only delete claims for game types they have authority over.
    /// The system should audit all claim deletion activities for security, accountability,
    /// and potential appeals processes.
    /// </remarks>
    public class DeleteUserClaim : IAuthorizationRequirement
    {
    }
}
