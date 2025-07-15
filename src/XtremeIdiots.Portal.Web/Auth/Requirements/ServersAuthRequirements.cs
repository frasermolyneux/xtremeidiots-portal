using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the public servers section of the portal.
    /// This permission controls access to view the list of enabled game servers, server
    /// information pages, and the global map displaying recent player activity.
    /// </summary>
    /// <remarks>
    /// The AccessServers requirement is designed for general community member access
    /// to public server information within the XtremeIdiots gaming portal. This includes
    /// viewing the list of active Call of Duty game servers, accessing individual server
    /// status pages, and viewing the community map showing recent player geo-locations.
    /// 
    /// This is typically a low-privilege requirement that most authenticated community
    /// members should have access to, as it provides read-only access to public server
    /// information. Unlike administrative server management permissions, this requirement
    /// focuses on community engagement and server discovery rather than server control.
    /// 
    /// The authorization handlers should verify that users have appropriate community
    /// membership status and are in good standing to access server information.
    /// </remarks>
    public class AccessServers : IAuthorizationRequirement
    {
    }
}
