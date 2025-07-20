using XtremeIdiots.Portal.Integrations.Forums.Models;

namespace XtremeIdiots.Portal.Integrations.Forums;

/// <summary>
/// Service for managing demo-related forum integrations
/// </summary>
public interface IDemoManager
{
    /// <summary>
    /// Retrieves information about the demo manager client
    /// </summary>
    /// <returns>A demo manager client data transfer object containing version and download information</returns>
    Task<DemoManagerClientDto> GetDemoManagerClient();
}