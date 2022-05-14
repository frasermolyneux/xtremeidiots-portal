using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosAuthApi
{
    public interface IDemosAuthApiClient
    {
        Task<List<DemoAuthDto>> CreateDemosAuths(string accessToken, List<DemoAuthDto> demoAuthDtos);
        Task<List<DemoAuthDto>> UpdateDemosAuths(string accessToken, List<DemoAuthDto> demoAuthDtos);

        Task<DemoAuthDto> CreateDemosAuth(string accessToken, DemoAuthDto demoAuthDto);
        Task<DemoAuthDto> UpdateDemosAuth(string accessToken, DemoAuthDto demoAuthDto);

        Task<DemoAuthDto?> GetDemosAuth(string accessToken, string userId);
        Task<DemoAuthDto?> GetDemosAuthByAuthKey(string accessToken, string authKey);
    }
}
