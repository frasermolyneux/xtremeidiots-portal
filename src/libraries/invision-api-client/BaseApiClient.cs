using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;

namespace XtremeIdiots.Portal.InvisionApiClient
{
    public class BaseApiClient
    {
        public BaseApiClient(ILogger logger, IOptions<InvisionApiClientOptions> options)
        {
            if (string.IsNullOrWhiteSpace(options.Value.BaseUrl))
                throw new ArgumentNullException(nameof(options.Value.BaseUrl));

            if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
                throw new ArgumentNullException(nameof(options.Value.ApiKey));

            RestClient = new RestClient(options.Value.BaseUrl)
            {
                Authenticator = new HttpBasicAuthenticator("", options.Value.ApiKey)
            };

            Logger = logger;
        }

        public ILogger Logger { get; }
        private RestClient RestClient { get; }

        public RestRequest CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);
            return request;
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            var response = await RestClient.ExecuteAsync(request);

            if (response.IsSuccessful)
                return response;

            if (response.StatusCode == HttpStatusCode.NotFound)
                return response;

            if (response.ErrorException != null)
            {
                Logger.LogError(response.ErrorException, $"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
                throw response.ErrorException;
            }

            Logger.LogError($"Failed {request.Method} to '{request.Resource}' with response status '{response.ResponseStatus}' and code '{response.StatusCode}'");
            throw new Exception($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
        }
    }
}