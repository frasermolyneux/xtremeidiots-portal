using System;
using System.Threading.Tasks;

namespace XI.Portal.Demos.Interfaces
{
    public interface IDemoFileRepository
    {
        Task CreateDemo(string fileName, string filePath);
        Task<Uri> GetDemoUrl(string fileName);
    }
}