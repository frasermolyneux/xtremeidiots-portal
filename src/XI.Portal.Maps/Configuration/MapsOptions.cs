using System;

namespace XI.Portal.Maps.Configuration
{
    public class MapsOptions : IMapsOptions
    {
        public string MapRedirectBaseUrl { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(MapRedirectBaseUrl))
                throw new NullReferenceException(nameof(MapRedirectBaseUrl));
        }
    }
}