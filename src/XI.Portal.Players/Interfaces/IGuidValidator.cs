namespace XI.Portal.Players.Interfaces
{
    public interface IGuidValidator
    {
        bool IsValid(string gameType, string guid);
    }
}