using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the website changelog functionality.
    /// Allows users to view the read-only display of application updates, feature releases,
    /// and version history. The changelog is a static informational page that shows
    /// system changes and improvements made to the XtremeIdiots Portal.
    /// </summary>
    public class AccessChangeLog : IAuthorizationRequirement
    {
    }
}
