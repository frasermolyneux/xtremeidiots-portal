using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.CodDemos.Models
{
    public interface IDemo
    {
        GameType Version { get; }
        string? Name { get; }
        DateTime Date { get; }
        string? Map { get; }
        string? Mod { get; }
        string? GameType { get; }
        string? Server { get; }
        long Size { get; }
        Stream Open();
    }
}