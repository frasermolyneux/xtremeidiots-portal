namespace XtremeIdiots.Portal.ServersApiClient;

public interface IServersApiTokenProvider
{
    Task<string> GetAccessToken();
}