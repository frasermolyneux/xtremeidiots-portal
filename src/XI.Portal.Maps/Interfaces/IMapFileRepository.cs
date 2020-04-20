using System;
using System.Threading.Tasks;

namespace XI.Portal.Maps.Interfaces
{
    public interface IMapFileRepository
    {
        Task<byte[]> GetFullRotationArchive(Guid id);
    }
}