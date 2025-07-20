using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.Integrations.Forums.Models;

namespace XtremeIdiots.Portal.Integrations.Forums;

public class DemoManager(IInvisionApiClient forumsClient) : IDemoManager
{
    private readonly IInvisionApiClient invisionClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));

    public async Task<DemoManagerClientDto> GetDemoManagerClient()
    {
        var downloadFile = await invisionClient.Downloads.GetDownloadFile(2753);

        return downloadFile is null
            ? throw new InvalidOperationException("Unable to retrieve demo manager download file from forums")
            : new DemoManagerClientDto
            {
                Version = downloadFile.Version ?? "Unknown",
                Description = downloadFile.Description ?? "No description available",
                Url = downloadFile.Url ?? throw new InvalidOperationException("Download URL is not available"),
                Changelog = downloadFile.Changelog ?? "No changelog available"
            };
    }
}