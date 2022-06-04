using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;

namespace XtremeIdiots.Portal.RepositoryApiClient.Interfaces
{
    public interface IDemosAuthApi
    {
        Task<List<DemoAuthDto>?> CreateDemosAuths(List<DemoAuthDto> demoAuthDtos);
        Task<List<DemoAuthDto>?> UpdateDemosAuths(List<DemoAuthDto> demoAuthDtos);

        Task<DemoAuthDto?> CreateDemosAuth(DemoAuthDto demoAuthDto);
        Task<DemoAuthDto?> UpdateDemosAuth(DemoAuthDto demoAuthDto);

        Task<DemoAuthDto?> GetDemosAuth(string userId);
        Task<DemoAuthDto?> GetDemosAuthByAuthKey(string authKey);
    }
}
