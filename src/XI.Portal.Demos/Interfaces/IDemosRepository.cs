using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Demos.Dto;
using XI.Portal.Demos.Models;

namespace XI.Portal.Demos.Interfaces
{
    public interface IDemosRepository
    {
        Task<List<DemoDto>> GetDemos(DemosFilterModel filterModel, ClaimsPrincipal user, string[] requiredClaims);
        Task<int> GetDemoCount(DemosFilterModel filterModel, ClaimsPrincipal user, string[] requiredClaims);
    }
}