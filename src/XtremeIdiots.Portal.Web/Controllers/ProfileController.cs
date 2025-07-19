using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing user profile information and settings
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessProfile)]
    public class ProfileController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

        public ProfileController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<ProfileController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Displays the user's profile management page
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The profile management view</returns>
        [HttpGet]
        public async Task<IActionResult> Manage(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                // The authorization is already handled by controller-level policy
                // No additional data retrieval needed for basic profile management page

                return await Task.FromResult(View());
            }, "ProfileManage");
        }
    }
}