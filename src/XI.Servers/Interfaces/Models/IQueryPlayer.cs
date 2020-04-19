namespace XI.Servers.Interfaces.Models
{
    public interface IQueryPlayer
    {
        string Name { get; set; }
        int Score { get; set; }

        string NormalizedName { get; }
    }
}