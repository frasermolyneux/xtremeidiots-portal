﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public class BaseApiClient
    {
        private readonly string _apimSubscriptionKey;

        public BaseApiClient(ILogger logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider)
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