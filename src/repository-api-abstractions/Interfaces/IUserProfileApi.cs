using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IUserProfileApi
    {
        Task<ApiResponseDto<UserProfileDto>> GetUserProfile(Guid userProfileId);
        Task<ApiResponseDto<UserProfileDto>> GetUserProfileByIdentityId(string identityId);
        Task<ApiResponseDto<UserProfileDto>> GetUserProfileByXtremeIdiotsId(string xtremeIdiotsId);
        Task<ApiResponseDto<UserProfileDto>> GetUserProfileByDemoAuthKey(string demoAuthKey);
        Task<ApiResponseDto<UserProfileCollectionDto>> GetUserProfiles(string? filterString, int skipEntries, int takeEntries, UserProfilesOrder? order);

        Task<ApiResponseDto> CreateUserProfile(CreateUserProfileDto createUserProfileClaimDto);
        Task<ApiResponseDto> CreateUserProfiles(List<CreateUserProfileDto> createUserProfileClaimDtos);

        Task<ApiResponseDto> UpdateUserProfile(EditUserProfileDto editUserProfileDto);
        Task<ApiResponseDto> UpdateUserProfiles(List<EditUserProfileDto> editUserProfileDtos);

        Task<ApiResponseDto> CreateUserProfileClaim(Guid userProfileId, List<CreateUserProfileClaimDto> createUserProfileClaimDto);
        Task<ApiResponseDto> SetUserProfileClaims(Guid userProfileId, List<CreateUserProfileClaimDto> createUserProfileClaimDto);

        Task<ApiResponseDto> DeleteUserProfileClaim(Guid userProfileId, Guid userProfileClaimId);
    }
}