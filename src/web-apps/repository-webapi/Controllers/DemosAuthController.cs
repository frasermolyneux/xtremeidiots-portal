using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class DemosAuthController : Controller, IDemosAuthApi
    {
        private readonly PortalDbContext context;
        private readonly IMapper mapper;

        public DemosAuthController(
            PortalDbContext context,
            IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [Route("api/demos-auth/{userId}")]
        public async Task<IActionResult> GetDemosAuth(string userId)
        {
            var response = await ((IDemosAuthApi)this).GetDemosAuth(userId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<DemoAuthDto>> IDemosAuthApi.GetDemosAuth(string userId)
        {
            var demoAuthKey = await context.DemoAuthKeys.SingleOrDefaultAsync(dak => dak.UserId == userId);

            if (demoAuthKey == null)
                return new ApiResponseDto<DemoAuthDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<DemoAuthDto>(demoAuthKey);

            return new ApiResponseDto<DemoAuthDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("api/demos-auth/by-auth-key/{authKey}")]
        public async Task<IActionResult> GetDemoAuthKeyByAuthKey(string authKey)
        {
            var response = await ((IDemosAuthApi)this).GetDemosAuthByAuthKey(authKey);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<DemoAuthDto>> IDemosAuthApi.GetDemosAuthByAuthKey(string authKey)
        {
            var demoAuthKey = await context.DemoAuthKeys.SingleOrDefaultAsync(dak => dak.AuthKey == authKey);

            if (demoAuthKey == null)
                return new ApiResponseDto<DemoAuthDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<DemoAuthDto>(demoAuthKey);

            return new ApiResponseDto<DemoAuthDto>(HttpStatusCode.OK, result);
        }

        Task<ApiResponseDto> IDemosAuthApi.CreateDemosAuth(CreateDemoAuthDto createDemoAuthDto)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("api/demos-auth")]
        public async Task<IActionResult> CreateDemoAuthKeys()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateDemoAuthDto>? createDemoAuthDto;
            try
            {
                createDemoAuthDto = JsonConvert.DeserializeObject<List<CreateDemoAuthDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createDemoAuthDto == null || !createDemoAuthDto.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IDemosAuthApi)this).CreateDemosAuths(createDemoAuthDto);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IDemosAuthApi.CreateDemosAuths(List<CreateDemoAuthDto> createDemoAuthDtos)
        {
            var demoAuthKeys = createDemoAuthDtos.Select(dak => mapper.Map<DemoAuthKey>(dak)).ToList();
            demoAuthKeys.ForEach(dak =>
            {
                dak.Created = DateTime.UtcNow;
                dak.LastActivity = DateTime.UtcNow;
            });

            await context.DemoAuthKeys.AddRangeAsync(demoAuthKeys);
            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        Task<ApiResponseDto> IDemosAuthApi.UpdateDemosAuth(EditDemoAuthDto editDemoAuthDto)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [Route("api/demos-auth")]
        public async Task<IActionResult> UpdateDemoAuthKeys()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<EditDemoAuthDto>? editDemoAuthDto;
            try
            {
                editDemoAuthDto = JsonConvert.DeserializeObject<List<EditDemoAuthDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (editDemoAuthDto == null || !editDemoAuthDto.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IDemosAuthApi)this).UpdateDemosAuths(editDemoAuthDto);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IDemosAuthApi.UpdateDemosAuths(List<EditDemoAuthDto> editDemoAuthDtos)
        {
            foreach (var editDemoAuthDto in editDemoAuthDtos)
            {
                var demoAuthKey = await context.DemoAuthKeys.SingleAsync(dak => dak.UserId == editDemoAuthDto.UserId);

                mapper.Map(editDemoAuthDto, demoAuthKey);

                demoAuthKey.LastActivity = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }
    }
}
