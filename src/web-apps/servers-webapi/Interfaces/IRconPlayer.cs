namespace XtremeIdiots.Portal.ServersWebApi.Interfaces
{
    public interface IRconPlayer
    {
        int Num { get; set; }
        string? Guid { get; set; }
        string? Name { get; set; }
        string? IpAddress { get; set; }
        int Rate { get; set; }
        int Ping { get; set; }
    }
}