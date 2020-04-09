using System;
using System.Threading.Tasks;

namespace XI.Portal.Maps.Repository
{
    public interface IMapFileRepository
    {
        Task<byte[]> GetFullRotationArchive(Guid id);
    }
}