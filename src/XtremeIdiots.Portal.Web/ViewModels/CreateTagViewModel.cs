using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Tags;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class CreateTagViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "HTML Markup")]
        public string? TagHtml { get; set; }

        [Display(Name = "User Defined")]
        public bool UserDefined { get; set; } = true;
    }
}
