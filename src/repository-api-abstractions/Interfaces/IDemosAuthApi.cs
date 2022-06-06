using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IDemosAuthApi
    {
        Task<ApiResponseDto<DemoAuthDto>> GetDemosAuth(string userId);
        Task<ApiResponseDto<DemoAuthDto>> GetDemosAuthByAuthKey(string authKey);

        Task<ApiResponseDto> CreateDemosAuth(CreateDemoAuthDto createDemoAuthDto);
        Task<ApiResponseDto> CreateDemosAuths(List<CreateDemoAuthDto> createDemoAuthDtos);

        Task<ApiResponseDto> UpdateDemosAuth(EditDemoAuthDto editDemoAuthDto);
        Task<ApiResponseDto> UpdateDemosAuths(List<EditDemoAuthDto> editDemoAuthDtos);
    }
}
