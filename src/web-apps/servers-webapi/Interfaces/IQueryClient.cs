namespace XtremeIdiots.Portal.ServersWebApi.Interfaces
{
    public interface IQueryClient
    {
        void Configure(string hostname, int queryPort);
        Task<IQueryResponse> GetServerStatus();
    }
}