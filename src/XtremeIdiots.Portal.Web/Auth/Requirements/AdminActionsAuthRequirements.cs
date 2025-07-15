using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the admin actions functionality.
    /// Allows users to view and browse admin actions but not modify them.
    /// </summary>
    public class AccessAdminActions : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for changing the administrator assigned to an admin action.
    /// Allows reassigning admin actions between different administrators.
    /// </summary>
    public class ChangeAdminActionAdmin : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for claiming an admin action.
    /// Allows administrators to take ownership of unassigned admin actions.
    /// </summary>
    public class ClaimAdminAction : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for creating new admin actions.
    /// Allows users to create bans, kicks, and other administrative actions against players.
    /// </summary>
    public class CreateAdminAction : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for creating forum topics related to admin actions.
    /// Allows integration with the community forums for admin action discussions.
    /// </summary>
    public class CreateAdminActionTopic : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting admin actions.
    /// Allows permanent removal of admin actions from the system.
    /// </summary>
    public class DeleteAdminAction : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for editing existing admin actions.
    /// Allows modification of admin action details such as reason, duration, or type.
    /// </summary>
    public class EditAdminAction : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for lifting (removing) admin actions.
    /// Allows early termination of bans or other temporary administrative actions.
    /// </summary>
    public class LiftAdminAction : IAuthorizationRequirement
    {
    }
}
