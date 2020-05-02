using System;
using System.Threading.Tasks;
using XI.Forums.Interfaces;
using XI.Portal.Demos.Dto;

namespace XI.Portal.Demos.Forums
{
    public class DemosForumsClient : IDemosForumsClient
    {
        private readonly IForumsClient _forumsClient;

        public DemosForumsClient(IForumsClient forumsClient)
        {
            _forumsClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));
        }

        public async Task<DemoManagerClientDto> GetDemoManagerClient()
        {
            var downloadFile = await _forumsClient.GetDownloadFile(2753);

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