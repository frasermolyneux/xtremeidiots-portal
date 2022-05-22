using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.CoreApi
{
    public class CoreApiClient : BaseApiClient, ICoreApiClient
    {
        public CoreApiClient(ILogger<CoreApiClient> logger, IOptions<InvisionApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<CoreHello?> GetCoreHello()
        {
            var request = CreateRequest("api/core/hello", Method.Get);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<CoreHello>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<Member?> GetMember(string id)
        {
            var request = CreateRequest($"api/core/members/{id}", Method.Get);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<Member>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
