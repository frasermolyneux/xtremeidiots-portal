using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class DemosAuthApi : BaseApi, IDemosAuthApi
    {
        public DemosAuthApi(ILogger<DemosAuthApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto<DemoAuthDto>> GetDemosAuth(string userId)
        {
            var request = await CreateRequest($"demos-auth/{userId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<DemoAuthDto>();
        }

        public async Task<ApiResponseDto<DemoAuthDto>> GetDemosAuthByAuthKey(string authKey)
        {
            var request = await CreateRequest($"demos-auth/by-auth-key/{authKey}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<DemoAuthDto>();
        }

        public async Task<ApiResponseDto> CreateDemosAuth(CreateDemoAuthDto createDemoAuthDto)
        {
            var request = await CreateRequest("demos-auth", Method.Post);
            request.AddJsonBody(new List<CreateDemoAuthDto> { createDemoAuthDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> CreateDemosAuths(List<CreateDemoAuthDto> createDemoAuthDtos)
        {
            var request = await CreateRequest("demos-auth", Method.Post);
            request.AddJsonBody(createDemoAuthDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateDemosAuth(EditDemoAuthDto editDemoAuthDto)
        {
            var request = await CreateRequest("demos-auth", Method.Put);
            request.AddJsonBody(new List<EditDemoAuthDto> { editDemoAuthDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateDemosAuths(List<EditDemoAuthDto> editDemoAuthDtos)
        {
            var request = await CreateRequest("demos-auth", Method.Put);
            request.AddJsonBody(editDemoAuthDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
