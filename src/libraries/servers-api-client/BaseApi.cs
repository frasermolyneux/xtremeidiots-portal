using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using System.Net;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public class BaseApi
    {
        private readonly string _apimSubscriptionKey;

        public BaseApi(ILogger logger, IOptions<ServersApiClientOptions> options, IServersApiTokenProvider serversApiTokenProvider)
        {
            if (string.IsNullOrWhiteSpace(options.Value.BaseUrl))
                throw new ArgumentNullException(nameof(options.Value.BaseUrl));

            if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
                throw new ArgumentNullException(nameof(options.Value.ApiKey));

            _apimSubscriptionKey = options.Value.ApiKey;

            RestClient = string.IsNullOrWhiteSpace(options.Value.ApiPathPrefix)
                ? new RestClient($"{options.Value.BaseUrl}")
                : new RestClient($"{options.Value.BaseUrl}/{options.Value.ApiPathPrefix}");

            Logger = logger;
            ServersApiTokenProvider = serversApiTokenProvider;
        }

        public ILogger Logger { get; }
        public IServersApiTokenProvider ServersApiTokenProvider { get; }
        private RestClient RestClient { get; }

        public async Task<RestRequest> CreateRequest(string resource, Method method)
        {
            var accessToken = await ServersApiTokenProvider.GetAccessToken();

            var request = new RestRequest(resource, method);

            request.AddHeader("Ocp-Apim-Subscription-Key", _apimSubscriptionKey);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

            return request;
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            var response = await RestClient.ExecuteAsync(request);

            if (response.ErrorException != null)
            {
                Logger.LogError(response.ErrorException, $"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
                throw response.ErrorException;
            }

            if (new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }.Contains(response.StatusCode))
            {
                return response;
            }
            else
            {
                Logger.LogError($"Failed {request.Method} to '{request.Resource}' with response status '{response.ResponseStatus}' and code '{response.StatusCode}'");
                throw new Exception($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
            }
        }
    }
}