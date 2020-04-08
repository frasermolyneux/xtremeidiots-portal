using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XI.Forums.Configuration;
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
    }
}