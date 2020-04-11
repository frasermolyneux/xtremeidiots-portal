using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Users.Models;

namespace XI.Portal.Users.Repository
{
    public interface IUsersRepository
    {
        Task<List<UserListEntryViewModel>> GetUsers();
    }
}