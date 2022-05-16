using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.DemosAuthApi
{
    public interface IDemosAuthApiClient
    {
        Task<List<DemoAuthDto>> CreateDemosAuths(List<DemoAuthDto> demoAuthDtos);
        Task<List<DemoAuthDto>> UpdateDemosAuths(List<DemoAuthDto> demoAuthDtos);

        Task<DemoAuthDto> CreateDemosAuth(DemoAuthDto demoAuthDto);
        Task<DemoAuthDto> UpdateDemosAuth(DemoAuthDto demoAuthDto);

        Task<DemoAuthDto?> GetDemosAuth(string userId);
        Task<DemoAuthDto?> GetDemosAuthByAuthKey(string authKey);
    }
}
