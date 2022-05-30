using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.ForumsApi
{
    public class ForumsApiClient : BaseApiClient, IForumsApiClient
    {
        public ForumsApiClient(ILogger<ForumsApiClient> logger, IOptions<InvisionApiClientOptions> options, TelemetryClient telemetryClient) : base(logger, options, telemetryClient)
        {
        }

        public async Task<PostTopicResult?> PostTopic(int forumId, int authorId, string title, string post, string prefix)
        {
            var request = CreateRequest($"api/forums/topics", Method.Post);

            request.AddParameter("forum", forumId);
            request.AddParameter("author", authorId);
            request.AddParameter("title", title);
            request.AddParameter("post", post);
            request.AddParameter("prefix", prefix);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<dynamic>(response.Content);

                if (result == null)
                    return null;

                return new PostTopicResult
                {
                    TopicId = result.id,
                    FirstPostId = result.firstPost.id
                };
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task UpdateTopic(int topicId, int authorId, string post)
        {
            var request = CreateRequest($"api/forums/topics/{topicId}", Method.Post);

            request.AddParameter("author", authorId);
            request.AddParameter("post", post);

            var response = await ExecuteAsync(request);
        }
    }
}
