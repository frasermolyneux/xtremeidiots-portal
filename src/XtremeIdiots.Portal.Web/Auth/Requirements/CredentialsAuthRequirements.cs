using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing the game server credentials page.
    /// This is a page-level authorization that allows users to view the credentials interface,
    /// but actual viewing of FTP and RCON credentials is further restricted by
    /// ViewFtpCredential and ViewRconCredential requirements from GameServersAuthRequirements.
    /// Users must have appropriate game-specific claims (SeniorAdmin, HeadAdmin, GameAdmin, 
    /// FtpCredentials, or RconCredentials) to see any credential information.
    /// </summary>
    public class AccessCredentials : IAuthorizationRequirement
    {
    }
}
