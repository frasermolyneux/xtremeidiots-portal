using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IDemosApi
    {
        Task<DemosSearchResponseDto?> SearchDemos(GameType[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, DemoOrder? order);
        Task<DemoDto?> GetDemo(Guid demoId);
        Task<DemoDto?> CreateDemo(CreateDemoDto demoDto, string fileName, string filePath);
        Task DeleteDemo(Guid demoId);
    }
}
