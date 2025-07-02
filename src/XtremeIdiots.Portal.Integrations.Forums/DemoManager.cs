using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.Integrations.Forums.Models;

namespace XtremeIdiots.Portal.Integrations.Forums
{
    public class DemoManager : IDemoManager
    {
        private readonly IInvisionApiClient _invisionClient;

        public DemoManager(IInvisionApiClient forumsClient)
        {
            _invisionClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));
        }

        public async Task<DemoManagerClientDto> GetDemoManagerClient()
        {
            var downloadFile = await _invisionClient.Downloads.GetDownloadFile(2753);

            return new DemoManagerClientDto
            {
                Version = downloadFile.Version,
                Description = downloadFile.Description,
                Url = downloadFile.Url,
                Changelog = downloadFile.Changelog
            };
        }
    }
}