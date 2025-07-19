using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.Integrations.Forums.Models;

namespace XtremeIdiots.Portal.Integrations.Forums
{
    /// <summary>
    /// Service for managing demo-related forum integrations
    /// </summary>
    public class DemoManager : IDemoManager
    {
        private readonly IInvisionApiClient _invisionClient;

        public DemoManager(IInvisionApiClient forumsClient)
        {
            _invisionClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));
        }

        /// <summary>
        /// Retrieves demo manager client information from the forums downloads section
        /// </summary>
        /// <returns>Demo manager client details including version and download information</returns>
        public async Task<DemoManagerClientDto> GetDemoManagerClient()
        {
            var downloadFile = await _invisionClient.Downloads.GetDownloadFile(2753);

            // Ensure we have valid download file data before proceeding
            if (downloadFile is null)
            {
                throw new InvalidOperationException("Unable to retrieve demo manager download file from forums");
            }

            return new DemoManagerClientDto
            {
                Version = downloadFile.Version ?? "Unknown",
                Description = downloadFile.Description ?? "No description available",
                Url = downloadFile.Url ?? throw new InvalidOperationException("Download URL is not available"),
                Changelog = downloadFile.Changelog ?? "No changelog available"
            };
        }
    }
}