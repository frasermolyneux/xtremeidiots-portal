using Newtonsoft.Json;

namespace XI.Forums.Models
{
    public class File
    {
        [JsonProperty("name")] public string Name { get; set; } 
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("size")] public long Size { get; set; }
    }
}