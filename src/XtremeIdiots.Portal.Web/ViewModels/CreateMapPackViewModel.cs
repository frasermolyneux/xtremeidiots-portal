using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class CreateMapPackViewModel
{
    public Guid GameServerId { get; set; }

    [Required]
    [DisplayName("Title")]
    public required string Title { get; set; }

    [Required]
    [DisplayName("Description")]
    public required string Description { get; set; }

    [DisplayName("Game Mode")]
    public required string GameMode { get; set; }

    [DisplayName("SyncToGameServer")]
    public bool SyncToGameServer { get; set; }
}