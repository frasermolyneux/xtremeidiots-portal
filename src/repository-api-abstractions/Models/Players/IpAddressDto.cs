namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class IpAddressDto
    {
        public string Address { get; set; } = string.Empty;
        public DateTime Added { get; set; }
        public DateTime LastUsed { get; set; }
    }
}