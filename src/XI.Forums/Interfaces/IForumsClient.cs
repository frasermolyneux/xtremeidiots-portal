using System.Threading.Tasks;
using XI.Forums.Models;

namespace XI.Forums.Interfaces
{
    public interface IForumsClient
    {
        Task<Member> GetMember(string id);
    }
}