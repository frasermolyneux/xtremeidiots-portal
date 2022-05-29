using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;

namespace XtremeIdiots.Portal.RepositoryApiClient.DemosRepositoryApi
{
    public interface IDemosApiClient
    {
        Task<DemosSearchResponseDto?> SearchDemos(string[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, string? order);
        Task<DemoDto?> GetDemo(Guid demoId);
        Task<DemoDto?> CreateDemo(CreateDemoDto demoDto, string fileName, string filePath);
        Task DeleteDemo(Guid demoId);
    }
}
