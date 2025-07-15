using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing system status and monitoring information.
    /// This permission controls access to operational status pages including ban file
    /// monitor status, system health information, and administrative oversight data.
    /// </summary>
    /// <remarks>
    /// The AccessStatus requirement is designed for administrative and monitoring purposes
    /// within the XtremeIdiots gaming portal. This includes viewing the status of ban file
    /// monitors that synchronize player ban lists from Call of Duty game servers, system
    /// health indicators, and other operational monitoring data.
    /// 
    /// This permission is typically granted to server administrators, moderators, and
    /// technical staff who need visibility into the operational status of the gaming
    /// platform infrastructure. The status information helps ensure proper functioning
    /// of automated systems like ban file synchronization, server monitoring, and
    /// community management tools.
    /// 
    /// Users with this permission can view ban file monitor statuses for game servers
    /// they have administrative access to, allowing them to verify that automated
    /// moderation systems are functioning correctly and ban lists are being properly
    /// synchronized across the Call of Duty server network.
    /// 
    /// Authorization handlers should verify that users have appropriate administrative
    /// privileges and access only to status information for systems they are authorized
    /// to manage or monitor.
    /// </remarks>
    public class AccessStatus : IAuthorizationRequirement
    {
    }
}
