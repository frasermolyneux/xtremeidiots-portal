using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class UserProfileApi : BaseApi, IUserProfileApi
    {
        public UserProfileApi(ILogger<UserProfileApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto<UserProfileDto>> GetUserProfile(Guid userProfileId)
        {
            var request = await CreateRequest($"user-profile/{userProfileId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<UserProfileDto>();
        }

        public async Task<ApiResponseDto<UserProfileDto>> GetUserProfileByIdentityId(string identityId)
        {
            var request = await CreateRequest($"user-profile/by-identity-id/{identityId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<UserProfileDto>();
        }

        public async Task<ApiResponseDto<UserProfileDto>> GetUserProfileByXtremeIdiotsId(string xtremeIdiotsId)
        {
            var request = await CreateRequest($"user-profile/by-xtremeidiots-id/{xtremeIdiotsId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<UserProfileDto>();
        }

        public async Task<ApiResponseDto<UserProfileDto>> GetUserProfileByDemoAuthKey(string demoAuthKey)
        {
            var request = await CreateRequest($"user-profile/by-demo-auth-key/{demoAuthKey}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<UserProfileDto>();
        }

        public async Task<ApiResponseDto<UserProfileCollectionDto>> GetUserProfiles(string? filterString, int skipEntries, int takeEntries, UserProfilesOrder? order)
        {
            var request = await CreateRequest("user-profile", Method.Get);

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString.ToString());

            request.AddQueryParameter("skipEntries", skipEntries.ToString());
            request.AddQueryParameter("takeEntries", takeEntries.ToString());

            if (order.HasValue)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<UserProfileCollectionDto>();
        }

        public async Task<ApiResponseDto> CreateUserProfile(CreateUserProfileDto createUserProfileDto)
        {
            var request = await CreateRequest("user-profile", Method.Post);
            request.AddJsonBody(new List<CreateUserProfileDto> { createUserProfileDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> CreateUserProfiles(List<CreateUserProfileDto> createUserProfileDtos)
        {
            var request = await CreateRequest("user-profile", Method.Post);
            request.AddJsonBody(createUserProfileDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateUserProfile(EditUserProfileDto editUserProfileDto)
        {
            var request = await CreateRequest("user-profile", Method.Put);
            request.AddJsonBody(new List<EditUserProfileDto> { editUserProfileDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateUserProfiles(List<EditUserProfileDto> editUserProfileDtos)
        {
            var request = await CreateRequest("user-profile", Method.Put);
            request.AddJsonBody(editUserProfileDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> CreateUserProfileClaim(Guid userProfileId, List<CreateUserProfileClaimDto> createUserProfileClaimDto)
        {
            var request = await CreateRequest($"user-profile/{userProfileId}/claims", Method.Patch);
            request.AddJsonBody(createUserProfileClaimDto);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> SetUserProfileClaims(Guid userProfileId, List<CreateUserProfileClaimDto> createUserProfileClaimDto)
        {
            var request = await CreateRequest($"user-profile/{userProfileId}/claims", Method.Post);
            request.AddJsonBody(createUserProfileClaimDto);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> DeleteUserProfileClaim(Guid userProfileId, Guid userProfileClaimId)
        {
            var request = await CreateRequest($"user-profile/{userProfileId}/claims/{userProfileClaimId}", Method.Delete);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
