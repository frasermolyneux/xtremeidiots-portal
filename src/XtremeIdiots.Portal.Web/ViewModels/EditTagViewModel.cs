using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for editing an existing tag
/// </summary>
public class EditTagViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the tag being edited
    /// </summary>
    [Required]
    public Guid TagId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the tag
    /// </summary>
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description explaining the tag's purpose
    /// </summary>
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the HTML markup used to render the tag in the UI
    /// </summary>
    [Display(Name = "HTML Markup")]
    public string? TagHtml { get; set; }

    /// <summary>
    /// Gets or sets whether this tag was created by a user (true) or is system-defined (false)
    /// </summary>
    [Display(Name = "User Defined")]
    public bool UserDefined { get; set; }
}