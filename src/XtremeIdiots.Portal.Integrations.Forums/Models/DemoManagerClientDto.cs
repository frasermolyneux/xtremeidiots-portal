namespace XtremeIdiots.Portal.Integrations.Forums.Models;

/// <summary>
/// Data transfer object containing information about the demo manager client
/// </summary>
public class DemoManagerClientDto
{
    /// <summary>
    /// Gets or sets the version of the demo manager client
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the description of the demo manager client
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the download URL for the demo manager client
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the changelog for the demo manager client
    /// </summary>
    public string? Changelog { get; set; }
}