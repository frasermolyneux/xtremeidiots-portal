using System.Threading.Tasks;
using XI.Forums.Models;

namespace XI.Forums.Client
{
    public interface IForumsClient
    {
        Task<Member> GetMember(string id);
    }
}