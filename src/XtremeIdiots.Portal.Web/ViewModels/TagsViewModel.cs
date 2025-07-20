using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for displaying a collection of tags
/// </summary>
public class TagsViewModel
{
    /// <summary>
    /// Gets or sets the list of tags to display
    /// </summary>
    public List<TagDto> Tags { get; set; } = [];
}