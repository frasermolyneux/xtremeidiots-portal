using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.DemosRepositoryApi
{
    public interface IDemosApiClient
    {
        Task<DemosSearchResponseDto> SearchDemos(string accessToken, string[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, string? order);
        Task<DemoDto?> GetDemo(string accessToken, Guid demoId);
        Task<DemoDto?> CreateDemo(string accessToken, DemoDto demoDto, string fileName, string filePath);
        Task DeleteDemo(string accessToken, Guid demoId);
    }
}
