using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.DemosAuthApi
{
    public class DemosAuthApiClient : BaseApiClient, IDemosAuthApiClient
    {
        public DemosAuthApiClient(ILogger<DemosAuthApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<DemoAuthDto> CreateDemosAuth(string accessToken, DemoAuthDto demoAuthDto)
        {
            var request = CreateRequest("repository/demos-auth", Method.Post, accessToken);
            request.AddJsonBody(new List<DemoAuthDto> { demoAuthDto });

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<DemoAuthDto>>(response.Content);
                return result?.FirstOrDefault() ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entity");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<DemoAuthDto>> CreateDemosAuths(string accessToken, List<DemoAuthDto> demoAuthDtos)
        {
            var request = CreateRequest("repository/demos-auth", Method.Post, accessToken);
            request.AddJsonBody(demoAuthDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<DemoAuthDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<DemoAuthDto?> GetDemosAuth(string accessToken, string userId)
        {
            var request = CreateRequest($"repository/demos-auth/{userId}", Method.Get, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemoAuthDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<DemoAuthDto?> GetDemosAuthByAuthKey(string accessToken, string authKey)
        {
            var request = CreateRequest($"repository/demos-auth/by-auth-key/{authKey}", Method.Get, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemoAuthDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<DemoAuthDto> UpdateDemosAuth(string accessToken, DemoAuthDto demoAuthDto)
        {
            var request = CreateRequest("repository/demos-auth", Method.Put, accessToken);
            request.AddJsonBody(new List<DemoAuthDto> { demoAuthDto });

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<DemoAuthDto>>(response.Content);
                return result?.FirstOrDefault() ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entity");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<DemoAuthDto>> UpdateDemosAuths(string accessToken, List<DemoAuthDto> demoAuthDtos)
        {
            var request = CreateRequest("repository/demos-auth", Method.Put, accessToken);
            request.AddJsonBody(demoAuthDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<DemoAuthDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
