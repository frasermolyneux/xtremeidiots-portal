using Newtonsoft.Json;
using System.Collections.Generic;

namespace XI.Forums.Models
{
    public class CustomField
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("fields")] public Dictionary<string, Field> Fields { get; set; }
    }
}