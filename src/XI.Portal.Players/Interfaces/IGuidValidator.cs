using XI.CommonTypes;

namespace XI.Portal.Players.Interfaces
{
    public interface IGuidValidator
    {
        bool IsValid(GameType gameType, string guid);
    }
}