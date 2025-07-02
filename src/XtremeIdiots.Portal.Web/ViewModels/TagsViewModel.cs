using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Tags;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class TagsViewModel
    {
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
    }
}
