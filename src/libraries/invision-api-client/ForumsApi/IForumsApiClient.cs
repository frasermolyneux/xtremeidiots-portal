using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.ForumsApi
{
    public interface IForumsApiClient
    {
        Task<PostTopicResult?> PostTopic(int forumId, int authorId, string title, string post, string prefix);
        Task UpdateTopic(int topicId, int authorId, string post);
    }
}