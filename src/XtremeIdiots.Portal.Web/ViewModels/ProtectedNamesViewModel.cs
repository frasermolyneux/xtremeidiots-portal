using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for displaying protected names data
/// </summary>
public class ProtectedNamesViewModel
{
    /// <summary>
    /// Initializes a new instance of the ProtectedNamesViewModel class
    /// </summary>
    public ProtectedNamesViewModel()
    {
        ProtectedNames = [];
    }

    /// <summary>
    /// Gets or sets the list of protected names
    /// </summary>
    public List<ProtectedNameDto> ProtectedNames { get; set; }
}