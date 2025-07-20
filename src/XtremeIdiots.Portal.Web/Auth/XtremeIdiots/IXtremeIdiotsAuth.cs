using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace XtremeIdiots.Portal.Web.Auth.XtremeIdiots;

/// <summary>
/// Service for handling XtremeIdiots external authentication integration
/// </summary>
public interface IXtremeIdiotsAuth
{
    /// <summary>
    /// Configures external authentication properties for redirecting to external provider
    /// </summary>
    /// <param name="redirectUrl">URL to redirect to after authentication</param>
    /// <returns>Configured authentication properties</returns>
    AuthenticationProperties ConfigureExternalAuthenticationProperties(string? redirectUrl);

    /// <summary>
    /// Retrieves external login information from the current request
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>External login information if available</returns>
    Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes the external login and creates or updates the local user account
    /// </summary>
    /// <param name="info">External login information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result with success status and any errors</returns>
    Task<XtremeIdiotsAuthResult> ProcessExternalLogin(ExternalLoginInfo info, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs out the current user from both local and external authentication schemes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completed task</returns>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}