using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Demos.Dto;
using XI.Portal.Demos.Models;

namespace XI.Portal.Demos.Interfaces
{
    public interface IDemosRepository
    {
        Task<int> GetDemosCount(DemosFilterModel filterModel);
        Task<List<DemoDto>> GetDemos(DemosFilterModel filterModel);
        Task<DemoDto> GetDemo(Guid demoId);
        Task CreateDemo(DemoDto demoDto, string filePath);
        //Task UpdateDemo(DemoDto demoDto);
        Task DeleteDemo(Guid demoId);
        Task<Uri> GetDemoUrl(Guid demoId);
    }
}