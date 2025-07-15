using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace XtremeIdiots.Portal.Web.Auth.XtremeIdiots
{
    /// <summary>
    /// Provides authentication services for XtremeIdiots external login integration.
    /// </summary>
    public interface IXtremeIdiotsAuth
    {
        /// <summary>
        /// Configures external authentication properties for XtremeIdiots login.
        /// </summary>
        /// <param name="redirectUrl">The optional URL to redirect to after authentication.</param>
        /// <returns>Authentication properties configured for XtremeIdiots provider.</returns>
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string? redirectUrl);

        /// <summary>
        /// Retrieves external login information from the current authentication context.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{ExternalLoginInfo}"/> representing the external login information, or null if no external login is in progress.</returns>
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes external login information and performs authentication.
        /// </summary>
        /// <param name="info">The external login information to process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{XtremeIdiotsAuthResult}"/> representing the authentication result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
        Task<XtremeIdiotsAuthResult> ProcessExternalLogin(ExternalLoginInfo info, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs out the current user.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous sign out operation.</returns>
        Task SignOutAsync(CancellationToken cancellationToken = default);
    }
}