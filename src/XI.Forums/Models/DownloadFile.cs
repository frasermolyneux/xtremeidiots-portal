using Newtonsoft.Json;
using System;

namespace XI.Forums.Models
{
    public class DownloadFile
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("category")] public Category Category { get; set; }
        [JsonProperty("author")] public Author Author { get; set; }
        [JsonProperty("date")] public DateTimeOffset Date { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("changelog")] public string Changelog { get; set; }
        [JsonProperty("files")] public File[] Files { get; set; }
        [JsonProperty("screenshots")] public object[] Screenshots { get; set; }
        [JsonProperty("primaryScreenshot")] public object PrimaryScreenshot { get; set; }
        [JsonProperty("downloads")] public long Downloads { get; set; }
        [JsonProperty("comments")] public long Comments { get; set; }
        [JsonProperty("reviews")] public long Reviews { get; set; }
        [JsonProperty("views")] public long Views { get; set; }
        [JsonProperty("prefix")] public object Prefix { get; set; }
        [JsonProperty("tags")] public object[] Tags { get; set; }
        [JsonProperty("locked")] public bool Locked { get; set; }
        [JsonProperty("hidden")] public bool Hidden { get; set; }
        [JsonProperty("pinned")] public bool Pinned { get; set; }
        [JsonProperty("featured")] public bool Featured { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("topic")] public object Topic { get; set; }
    }
}