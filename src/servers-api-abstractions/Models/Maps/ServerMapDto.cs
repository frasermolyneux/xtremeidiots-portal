using Newtonsoft.Json;

namespace XtremeIdiots.Portal.ServersApi.Abstractions.Models.Maps
{
    public class ServerMapDto
    {
        public ServerMapDto(string name, string fullName, DateTime modified)
        {
            Name = name;
            FullName = fullName;
            Modified = modified;
        }

        [JsonProperty]
        public string Name { get; internal set; }

        [JsonProperty]
        public string FullName { get; internal set; }

        [JsonProperty]
        public DateTime Modified { get; internal set; }
    }
}
