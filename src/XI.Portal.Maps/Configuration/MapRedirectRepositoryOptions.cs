using System;
using XI.Portal.Maps.Interfaces;

namespace XI.Portal.Maps.Configuration
{
    public class MapRedirectRepositoryOptions : IMapRedirectRepositoryOptions
    {
        public string MapRedirectBaseUrl { get; set; }
        public string ApiKey { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(MapRedirectBaseUrl))
                throw new NullReferenceException(nameof(MapRedirectBaseUrl));

            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new NullReferenceException(nameof(ApiKey));
        }
    }
}