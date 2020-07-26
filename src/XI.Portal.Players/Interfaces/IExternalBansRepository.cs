using System.Threading.Tasks;
using XI.CommonTypes;

namespace XI.Portal.Players.Interfaces
{
    public interface IExternalBansRepository
    {
        Task GetBanFileForGame(GameType gameType);
    }
}