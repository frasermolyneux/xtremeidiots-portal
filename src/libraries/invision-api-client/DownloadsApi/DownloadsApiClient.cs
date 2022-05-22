using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.DownloadsApi
{
    public class DownloadsApiClient : BaseApiClient, IDownloadsApiClient
    {
        public DownloadsApiClient(ILogger<DownloadsApiClient> logger, IOptions<InvisionApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<DownloadFile?> GetDownloadFile(int fileId)
        {
            var request = CreateRequest($"api/downloads/files/{fileId}", Method.Get);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DownloadFile>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
