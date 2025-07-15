using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the demo files functionality.
    /// Allows users to view and browse demo files from supported game types
    /// (Call of Duty 2, 4, and 5). Users can access demos from games they have 
    /// permissions for and download demo files for replay viewing.
    /// </summary>
    public class AccessDemos : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization requirement for deleting demo files.
    /// Allows users to delete demo files based on ownership and game type permissions:
    /// - Regular users can delete their own demo files
    /// - Senior admins and head admins can delete any demo files for their authorized game types
    /// Authorization checks both the game type and the user profile who uploaded the demo.
    /// </summary>
    public class DeleteDemo : IAuthorizationRequirement
    {
    }
}
