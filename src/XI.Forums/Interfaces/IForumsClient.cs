using System.Threading.Tasks;
using XI.Forums.Models;

namespace XI.Forums.Interfaces
{
    public interface IForumsClient
    {
        Task<Member> GetMember(string id);
        Task<PostTopicResult> PostTopic(int forumId, int authorId, string title, string post, string prefix);
        Task UpdateTopic(int topicId, int authorId, string post);
        Task<DownloadFile> GetDownloadFile(int fileId);
        Task<CoreHello> GetCoreHello();
    }
}