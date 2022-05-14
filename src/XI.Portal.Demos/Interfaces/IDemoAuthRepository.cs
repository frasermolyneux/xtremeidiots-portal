using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Demos.Models;

namespace XI.Portal.Demos.Interfaces
{
    public interface IDemoAuthRepository
    {
        Task<string> GetAuthKey(string userId);
        Task UpdateAuthKey(string userId, string authKey);
        Task<string> GetUserId(string authKey);
        Task<List<DemoAuthEntity>> GetAllAuthKeys();
    }
}