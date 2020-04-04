using System.Threading.Tasks;
using XI.Forums.Models;

namespace XI.Forums
{
    public interface IForumsClient
    {
        Task<Member> GetMember(string id);
    }
}