using System;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosRepositoryApi
{
    public interface IDemosApiClient
    {
        Task<DemosSearchResponseDto> SearchDemos(string[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, string? order);
        Task<DemoDto?> GetDemo(Guid demoId);
        Task<DemoDto?> CreateDemo(DemoDto demoDto, string fileName, string filePath);
        Task DeleteDemo(Guid demoId);
    }
}
