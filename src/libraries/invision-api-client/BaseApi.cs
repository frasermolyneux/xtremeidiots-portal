using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using System.Net;
using System.Text;

namespace XtremeIdiots.Portal.InvisionApiClient
{
    public class BaseApi
    {
        private readonly IOptions<InvisionApiClientOptions> options;
        private readonly TelemetryClient telemetryClient;

        public BaseApi(ILogger logger, IOptions<InvisionApiClientOptions> options, TelemetryClient telemetryClient)
        {
            if (string.IsNullOrWhiteSpace(options.Value.BaseUrl))
                throw new ArgumentNullException(nameof(options.Value.BaseUrl));

            if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
                throw new ArgumentNullException(nameof(options.Value.ApiKey));

            RestClient = string.IsNullOrWhiteSpace(options.Value.ApiPathPrefix)
                ? new RestClient($"{options.Value.BaseUrl}")
                : new RestClient($"{options.Value.BaseUrl}/{options.Value.ApiPathPrefix}");

            Logger = logger;
            this.options = options;
            this.telemetryClient = telemetryClient;
        }

        public ILogger Logger { get; }
        private RestClient RestClient { get; }

        public RestRequest CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);

#pragma warning disable CS8604 // Possible null reference argument.
            request.AddHeader("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(options.Value.ApiKey))}");
#pragma warning restore CS8604 // Possible null reference argument.

            return request;
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("InvisionRestApi");
            operation.Telemetry.Type = "HTTP";
            operation.Telemetry.Target = options.Value.BaseUrl;
            operation.Telemetry.Data = request.Resource;

            try
            {
                var response = await RestClient.ExecuteAsync(request);

                if (response.IsSuccessful)
                    return response;

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return response;

                if (response.ErrorException != null)
                {
                    operation.Telemetry.Success = false;
                    operation.Telemetry.ResultCode = response.ErrorException.Message;
                    telemetryClient.TrackException(response.ErrorException);

                    throw response.ErrorException;
                }

                Logger.LogError($"Failed {request.Method} to '{request.Resource}' with response status '{response.ResponseStatus}' and code '{response.StatusCode}'");
                throw new Exception($"Failed {request.Method} to '{request.Resource}' with code '{response.StatusCode}'");
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }
    }
}