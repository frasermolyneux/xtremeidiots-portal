using Microsoft.Extensions.Options;
using RestSharp;

namespace XtremeIdiots.Portal.RepositoryApiClient;

public class BaseApiClient
{
    private readonly string _apimSubscriptionKey;

    public BaseApiClient(IOptions<RepositoryApiClientOptions> options)
    {
        _apimSubscriptionKey = options.Value.ApimSubscriptionKey;

        RestClient = new RestClient(options.Value.ApimBaseUrl);
    }

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
        return await RestClient.ExecuteAsync(request);
    }
}