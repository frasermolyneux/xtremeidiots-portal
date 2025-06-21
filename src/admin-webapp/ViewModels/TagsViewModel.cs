using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Tags;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class TagsViewModel
    {
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
    }
}
