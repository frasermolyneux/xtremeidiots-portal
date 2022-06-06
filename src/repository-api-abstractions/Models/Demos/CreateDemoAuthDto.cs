using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos
{
    public class CreateDemoAuthDto
    {
        public CreateDemoAuthDto(string userId, string authKey)
        {
            UserId = userId;
            AuthKey = authKey;
        }

        [JsonProperty]
        public string UserId { get; private set; }

        [JsonProperty]
        public string AuthKey { get; private set; }
    }
}
