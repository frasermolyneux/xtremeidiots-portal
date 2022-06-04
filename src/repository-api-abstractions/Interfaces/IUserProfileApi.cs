using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IUserProfileApi
    {
        Task<UserProfileResponseDto?> GetUserProfiles(int skipEntries, int takeEntries, string? filterString);
        Task<UserProfileDto?> GetUserProfile(Guid userProfileId);
        Task<UserProfileDto?> GetUserProfileByIdentityId(string identityId);
        Task<UserProfileDto?> GetUserProfileByXtremeIdiotsId(string xtremeIdiotsId);
        Task<List<UserProfileDto>?> CreateUserProfiles(List<UserProfileDto> userProfileDtos);
        Task<UserProfileDto?> CreateUserProfile(UserProfileDto userProfileDto);
        Task<List<UserProfileDto>?> UpdateUserProfiles(List<UserProfileDto> userProfileDtos);
        Task<UserProfileDto?> UpdateUserProfile(UserProfileDto userProfileDto);
        Task<List<UserProfileClaimDto>?> GetUserProfileClaims(Guid userProfileId);
        Task<List<UserProfileClaimDto>?> CreateUserProfileClaims(Guid userProfileId, List<UserProfileClaimDto> userProfileClaimDtos);
    }
}