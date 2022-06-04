using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using System.Net;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class UserProfileApi : BaseApi, IUserProfileApi
    {
        public UserProfileApi(ILogger<UserProfileApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<UserProfileResponseDto?> GetUserProfiles(int skipEntries, int takeEntries, string? filterString)
        {
            var request = await CreateRequest($"repository/user-profile", Method.Get);

            request.AddQueryParameter("skipEntries", skipEntries.ToString());
            request.AddQueryParameter("takeEntries", takeEntries.ToString());

            if (!string.IsNullOrEmpty(filterString))
                request.AddQueryParameter("filterString", filterString);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<UserProfileResponseDto>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<UserProfileDto?> CreateUserProfile(UserProfileDto userProfileDto)
        {
            var request = await CreateRequest($"repository/user-profile", Method.Post);
            request.AddJsonBody(new List<UserProfileDto> { userProfileDto });

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<UserProfileDto>>(response.Content);
                return result?.FirstOrDefault() ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entity");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<UserProfileClaimDto>?> CreateUserProfileClaims(Guid userProfileId, List<UserProfileClaimDto> userProfileClaimDtos)
        {
            var request = await CreateRequest($"repository/user-profile/{userProfileId}/claims", Method.Post);
            request.AddJsonBody(userProfileClaimDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<UserProfileClaimDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<UserProfileDto>?> CreateUserProfiles(List<UserProfileDto> userProfileDtos)
        {
            var request = await CreateRequest($"repository/user-profile", Method.Post);
            request.AddJsonBody(userProfileDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<UserProfileDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<UserProfileDto?> GetUserProfile(Guid userProfileId)
        {
            var request = await CreateRequest($"repository/user-profile/{userProfileId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<UserProfileDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<UserProfileDto?> GetUserProfileByIdentityId(string identityId)
        {
            var request = await CreateRequest($"repository/user-profile/by-identity-id/{identityId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<UserProfileDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<UserProfileDto?> GetUserProfileByXtremeIdiotsId(string xtremeIdiotsId)
        {
            var request = await CreateRequest($"repository/user-profile/by-xtremeidiots-id/{xtremeIdiotsId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<UserProfileDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<UserProfileClaimDto>?> GetUserProfileClaims(Guid userProfileId)
        {
            var request = await CreateRequest($"repository/user-profile/{userProfileId}/claims", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<UserProfileClaimDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<UserProfileDto?> UpdateUserProfile(UserProfileDto userProfileDto)
        {
            var request = await CreateRequest($"repository/user-profile", Method.Put);
            request.AddJsonBody(new List<UserProfileDto> { userProfileDto });

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<UserProfileDto>>(response.Content);
                return result?.FirstOrDefault() ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entity");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<UserProfileDto>?> UpdateUserProfiles(List<UserProfileDto> userProfileDtos)
        {
            var request = await CreateRequest($"repository/user-profile", Method.Post);
            request.AddJsonBody(userProfileDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<UserProfileDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
