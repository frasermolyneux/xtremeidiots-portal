using Newtonsoft.Json;

namespace XI.Forums.Models
{
    public class Member
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("timeZone")] public string TimeZone { get; set; }

        [JsonProperty("formattedName")] public string FormattedName { get; set; }

        [JsonProperty("primaryGroup")] public Group PrimaryGroup { get; set; }

        [JsonProperty("secondaryGroups")] public Group[] SecondaryGroups { get; set; }

        [JsonProperty("email")] public string Email { get; set; }

        [JsonProperty("joined")] public DateTimeOffset? Joined { get; set; }

        [JsonProperty("registrationIpAddress")]
        public string RegistrationIpAddress { get; set; }

        [JsonProperty("warningPoints")] public long WarningPoints { get; set; }

        [JsonProperty("reputationPoints")] public long ReputationPoints { get; set; }

        [JsonProperty("photoUrl")] public string PhotoUrl { get; set; }

        [JsonProperty("photoUrlIsDefault")] public bool PhotoUrlIsDefault { get; set; }

        [JsonProperty("coverPhotoUrl")] public string CoverPhotoUrl { get; set; }

        [JsonProperty("profileUrl")] public Uri ProfileUrl { get; set; }

        [JsonProperty("validating")] public bool Validating { get; set; }

        [JsonProperty("posts")] public long Posts { get; set; }

        [JsonProperty("lastActivity")] public DateTimeOffset? LastActivity { get; set; }

        [JsonProperty("lastVisit")] public DateTimeOffset? LastVisit { get; set; }

        [JsonProperty("lastPost")] public DateTimeOffset? LastPost { get; set; }

        [JsonProperty("profileViews")] public long ProfileViews { get; set; }

        [JsonProperty("birthday")] public string Birthday { get; set; }

        [JsonProperty("customFields")] public Dictionary<string, CustomField> CustomFields { get; set; }
    }
}