using System.Collections.Generic;
using Newtonsoft.Json;

namespace XI.Forums.Models
{
    public class CustomField
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("fields")] public Dictionary<string, Field> Fields { get; set; }
    }
}