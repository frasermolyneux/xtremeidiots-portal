using System.Threading.Tasks;
using XI.Portal.Demos.Models;

namespace XI.Portal.Demos.Repository
{
    public interface IDemosRepository
    {
        Task<IDemoDto> GetUserDemo(string userId, string demoId);
        Task UpdateDemo(IDemoDto demoDto);
    }
}