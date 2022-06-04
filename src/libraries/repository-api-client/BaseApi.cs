using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using System.Net;

namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public class BaseApi
    {
        private readonly string _apimSubscriptionKey;

        public BaseApi(ILogger logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider)
        {
            _apimSubscriptionKey = options.Value.ApimSubscriptionKey;

            RestClient = new RestClient(options.Value.ApimBaseUrl);
            Logger = logger;
            RepositoryApiTokenProvider = repositoryApiTokenProvider;
        }

        public ILogger Logger { get; }
        public IRepositoryApiTokenProvider RepositoryApiTokenProvider { get; }
        private RestClient RestClient { get; }

        public async Task<RestRequest> CreateRequest(string resource, Method method)
        {
            var accessToken = await RepositoryApiTokenProvider.GetAccessToken();

            var request = new RestRequest(resource, method);

            request.AddHeader("Ocp-Apim-Subscription-Key", _apimSubscriptionKey);
            request.AddHeader("Authorization", $"Bearer {accessToken}");

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