using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the application home page.
    /// This requirement is designed to provide public access to the main entry point
    /// of the XtremeIdiots Portal. The associated HomeAuthHandler automatically
    /// succeeds this requirement for all users, effectively making the home page
    /// publicly accessible while maintaining the authorization framework consistency.
    /// </summary>
    public class AccessHome : IAuthorizationRequirement
    {
    }
}
