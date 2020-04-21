using System;
using System.Collections.Generic;
using FM.GeoLocation.Client;

namespace XI.Portal.Players.Configuration
{
    public class GeoLocationClientConfig : IGeoLocationClientConfiguration
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }

        public bool UseMemoryCache { get; } = true;
        public int CacheEntryLifeInMinutes { get; } = 60;

        public IEnumerable<TimeSpan> RetryTimespans
        {
            get
            {
                var random = new Random();

                return new[]
                {
                    TimeSpan.FromSeconds(random.Next(1)),
                    TimeSpan.FromSeconds(random.Next(3)),
                    TimeSpan.FromSeconds(random.Next(5))
                };
            }
        }
    }
}