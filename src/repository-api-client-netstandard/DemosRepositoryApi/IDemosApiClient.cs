using System;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosRepositoryApi
{
    public interface IDemosApiClient
    {
        Task<DemosSearchResponseDto> SearchDemos(string accessToken, string[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, string? order);
        Task<DemoDto> GetDemo(string accessToken, Guid demoId);
        Task<DemoDto> CreateDemo(string accessToken, DemoDto demoDto);
        Task DeleteDemo(string accessToken, Guid demoId);
    }
}
