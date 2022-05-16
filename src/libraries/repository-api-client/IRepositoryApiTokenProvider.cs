namespace XtremeIdiots.Portal.RepositoryApiClient;

public interface IRepositoryApiTokenProvider
{
    Task<string> GetAccessToken();
}