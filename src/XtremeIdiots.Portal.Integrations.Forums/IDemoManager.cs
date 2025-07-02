using XtremeIdiots.Portal.Integrations.Forums.Models;

namespace XtremeIdiots.Portal.Integrations.Forums
{
    public interface IDemoManager
    {
        Task<DemoManagerClientDto> GetDemoManagerClient();
    }
}