using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for editing an existing ban file monitor
/// </summary>
public class EditBanFileMonitorViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the ban file monitor
    /// </summary>
    public Guid BanFileMonitorId { get; set; }

    /// <summary>
    /// Gets or sets the file path of the ban file being monitored
    /// </summary>
    [Required]
    [DisplayName("File Path")]
    public required string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the size of the remote ban file in bytes
    /// </summary>
    [DisplayName("Remote File Size")]
    public long? RemoteFileSize { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the ban file was last synchronized
    /// </summary>
    [DisplayName("Last Read")]
    public DateTime? LastSync { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the game server
    /// </summary>
    [DisplayName("Server")]
    public Guid GameServerId { get; set; }

    /// <summary>
    /// Gets or sets the game server information for display purposes
    /// </summary>
    [DisplayName("Server")]
    public GameServerDto? GameServer { get; set; }
}