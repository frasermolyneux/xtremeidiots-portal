namespace XtremeIdiots.Portal.FuncHelpers.Providers;

public interface IServersApiTokenProvider
{
    Task<string> GetAccessToken();
}