using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.CoreApi
{
    public interface ICoreApiClient
    {
        Task<Member?> GetMember(string id);
        Task<CoreHello?> GetCoreHello();
    }
}