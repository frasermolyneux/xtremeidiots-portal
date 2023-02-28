using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.XtremeIdiots
{
    public interface IXtremeIdiotsAuth
    {
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string? redirectUrl);
        Task<ExternalLoginInfo> GetExternalLoginInfoAsync();
        Task<XtremeIdiotsAuthResult> ProcessExternalLogin(ExternalLoginInfo info);
        Task SignOutAsync();
    }
}