using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for creating a new tag
/// </summary>
public class CreateTagViewModel
{
    /// <summary>
    /// Gets or sets the name of the tag
    /// </summary>
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the tag
    /// </summary>
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the HTML markup for the tag display
    /// </summary>
    [Display(Name = "HTML Markup")]
    public string? TagHtml { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tag is user-defined
    /// </summary>
    [Display(Name = "User Defined")]
    public bool UserDefined { get; set; } = true;
}