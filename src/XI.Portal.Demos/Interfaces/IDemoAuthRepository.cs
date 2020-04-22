using System.Threading.Tasks;

namespace XI.Portal.Demos.Interfaces
{
    public interface IDemoAuthRepository
    {
        Task<string> GetAuthKey(string userId);
        Task UpdateAuthKey(string userId, string authKey);
    }
}