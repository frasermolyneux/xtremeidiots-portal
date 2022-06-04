using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.Interfaces
{
    public interface ICoreApi
    {
        Task<Member?> GetMember(string id);
        Task<CoreHello?> GetCoreHello();
    }
}