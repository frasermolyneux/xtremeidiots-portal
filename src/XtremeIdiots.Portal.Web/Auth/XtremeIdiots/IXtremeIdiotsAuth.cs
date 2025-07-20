using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace XtremeIdiots.Portal.Web.Auth.XtremeIdiots;

public interface IXtremeIdiotsAuth
{

    AuthenticationProperties ConfigureExternalAuthenticationProperties(string? redirectUrl);

    Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(CancellationToken cancellationToken = default);

    Task<XtremeIdiotsAuthResult> ProcessExternalLogin(ExternalLoginInfo info, CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);
}