using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for creating a new ban file monitor
/// </summary>
public class CreateBanFileMonitorViewModel
{
    /// <summary>
    /// Gets or sets the file path to monitor on the server
    /// </summary>
    [Required]
    [DisplayName("File Path")]
    public required string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the remote file size for tracking changes
    /// </summary>
    [DisplayName("Remote File Size")]
    public long RemoteFileSize { get; set; }

    /// <summary>
    /// Gets or sets the last synchronization timestamp
    /// </summary>
    [DisplayName("Last Read")]
    public DateTime LastSync { get; set; }

    /// <summary>
    /// Gets or sets the game server ID this monitor belongs to
    /// </summary>
    [DisplayName("Server")]
    public Guid GameServerId { get; set; }

    /// <summary>
    /// Gets or sets the game server details for display
    /// </summary>
    [DisplayName("Server")]
    public GameServerDto? GameServer { get; set; }
}