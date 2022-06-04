using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.Interfaces
{
    public interface IForumsApi
    {
        Task<PostTopicResult?> PostTopic(int forumId, int authorId, string title, string post, string prefix);
        Task UpdateTopic(int topicId, int authorId, string post);
    }
}