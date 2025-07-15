using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the ban file monitors functionality.
    /// Allows users to view and browse ban file monitors but not modify them.
    /// </summary>
    public class AccessBanFileMonitors : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for viewing details of a specific ban file monitor.
    /// Allows users to examine ban file monitor configuration and status.
    /// </summary>
    public class ViewBanFileMonitor : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for creating new ban file monitors.
    /// Allows users to set up automated monitoring of game server ban files.
    /// </summary>
    public class CreateBanFileMonitor : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for editing existing ban file monitors.
    /// Allows modification of monitor settings, file paths, and configuration options.
    /// </summary>
    public class EditBanFileMonitor : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting ban file monitors.
    /// Allows permanent removal of ban file monitoring configurations.
    /// </summary>
    public class DeleteBanFileMonitor : IAuthorizationRequirement
    {
    }
}
