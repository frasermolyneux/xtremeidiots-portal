namespace XI.Servers.Query.Models
{
    public interface IQueryPlayer
    {
        string Name { get; set; }
        int Score { get; set; }

        string NormalizedName { get; }
    }
}