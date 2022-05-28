using XtremeIdiots.Portal.ForumsIntegration.Models;

namespace XtremeIdiots.Portal.ForumsIntegration
{
    public interface IDemoManager
    {
        Task<DemoManagerClientDto> GetDemoManagerClient();
    }
}