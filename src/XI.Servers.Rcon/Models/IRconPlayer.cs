namespace XI.Servers.Rcon.Models
{
    public interface IRconPlayer
    {
        string Num { get; set; }
        string Guid { get; set; }
        string Name { get; set; }
        string IpAddress { get; set; }
        string Rate { get; set; }

        string NormalizedName { get; }
    }
}