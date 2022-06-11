using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IDemosApi
    {
        Task<ApiResponseDto<DemoDto>> GetDemo(Guid demoId);
        Task<ApiResponseDto<DemosCollectionDto>> GetDemos(GameType[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, DemoOrder? order);

        Task<ApiResponseDto<DemoDto>> CreateDemo(CreateDemoDto createDemoDto);

        Task<ApiResponseDto> SetDemoFile(Guid demoId, string fileName, string filePath);

        Task<ApiResponseDto> DeleteDemo(Guid demoId);
    }
}
