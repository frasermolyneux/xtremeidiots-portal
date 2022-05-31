namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class IpAddressDto
    {
        public string Address { get; set; } = string.Empty;
        public DateTime Added { get; set; }
        public DateTime LastUsed { get; set; }
    }
}