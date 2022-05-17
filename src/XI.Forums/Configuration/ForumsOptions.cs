using XI.Forums.Interfaces;

namespace XI.Forums.Configuration
{
    internal class ForumsOptions : IForumsOptions
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
                throw new NullReferenceException(nameof(BaseUrl));

            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new NullReferenceException(nameof(ApiKey));
        }
    }
}