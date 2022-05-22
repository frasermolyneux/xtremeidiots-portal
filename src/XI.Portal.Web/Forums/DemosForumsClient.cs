using System;
using System.Threading.Tasks;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.InvisionApiClient;

namespace XI.Portal.Web.Forums
{
    public class DemosForumsClient : IDemosForumsClient
    {
        private readonly IInvisionApiClient _invisionClient;

        public DemosForumsClient(IInvisionApiClient forumsClient)
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

    public interface IDemosForumsClient
    {
        Task<DemoManagerClientDto> GetDemoManagerClient();
    }
}