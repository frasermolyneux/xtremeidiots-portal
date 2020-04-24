using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XI.Forums.Interfaces;
using XI.Forums.Models;

namespace XI.Forums.Client
{
    public class ForumsClient : IForumsClient
    {
        private readonly IForumsOptions _forumsOptions;

        public ForumsClient(IForumsOptions forumsOptions)
        {
            _forumsOptions = forumsOptions ?? throw new ArgumentNullException(nameof(forumsOptions));
        }

        public async Task<Member> GetMember(string id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_forumsOptions.BaseUrl);
                var byteArray = Encoding.ASCII.GetBytes(_forumsOptions.ApiKey);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var result = await client.GetStringAsync($"/api/core/members/{id}");

                return JsonConvert.DeserializeObject<Member>(result);
            }
        }

        public async Task<PostTopicResult> PostTopic(int forumId, int authorId, string title, string post, string prefix)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_forumsOptions.BaseUrl);

                var requestParams = new Dictionary<string, string>()
                {
                    {"key", _forumsOptions.ApiKey},
                    {"forum", forumId.ToString()},
                    {"author", authorId.ToString()},
                    {"title", title},
                    {"post", post},
                    {"prefix", prefix}
                };

                var response = await client.PostAsync("/api/forums/topics", new FormUrlEncodedContent(requestParams));
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseAsDynamic = JsonConvert.DeserializeObject<dynamic>(responseBody);

                return new PostTopicResult
                {
                    TopicId = responseAsDynamic.id,
                    FirstPostId = responseAsDynamic.firstPost.id
                };
            }
        }
    }
}