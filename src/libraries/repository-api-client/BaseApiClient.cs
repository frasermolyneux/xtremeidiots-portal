using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Net;

namespace XtremeIdiots.Portal.RepositoryApiClient;

public class BaseApiClient
{
    private readonly string _apimSubscriptionKey;

    public BaseApiClient(ILogger logger, IOptions<RepositoryApiClientOptions> options)
    {
        _apimSubscriptionKey = options.Value.ApimSubscriptionKey;

        RestClient = new RestClient(options.Value.ApimBaseUrl);
        Logger = logger;
    }

    public ILogger Logger { get; }
    private RestClient RestClient { get; }

    public RestRequest CreateRequest(string resource, Method method, string accessToken)
    {
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