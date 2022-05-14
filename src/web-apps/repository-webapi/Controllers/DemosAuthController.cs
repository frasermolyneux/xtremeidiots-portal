using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class DemosAuthController : Controller
    {
        public DemosAuthController(PortalDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PortalDbContext Context { get; }

        [HttpGet]
        [Route("api/demos-auth/{userId}")]
        public async Task<IActionResult> GetDemoAuthKeyByUserId(string userId)
        {
            var demoAuthKey = await Context.DemoAuthKeys.SingleOrDefaultAsync(dak => dak.UserId == userId);

            if (demoAuthKey == null)
                return NotFound();

            var dto = demoAuthKey.ToDto();

            return new OkObjectResult(dto);
        }

        [HttpGet]
        [Route("api/demos-auth/by-auth-key/{authKey}")]
        public async Task<IActionResult> GetDemoAuthKeyByAuthKey(string authKey)
        {
            var demoAuthKey = await Context.DemoAuthKeys.SingleOrDefaultAsync(dak => dak.AuthKey == authKey);

            if (demoAuthKey == null)
                return NotFound();

            var dto = demoAuthKey.ToDto();

            return new OkObjectResult(dto);
        }

        [HttpPost]
        [Route("api/demos-auth")]
        public async Task<IActionResult> CreateDemoAuthKeys()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<DemoAuthDto>? demoAuthDtos;
            try
            {
                demoAuthDtos = JsonConvert.DeserializeObject<List<DemoAuthDto>>(requestBody);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            if (demoAuthDtos == null || demoAuthDtos.Count == 0)
                return new BadRequestResult();

            var demoAuthKeys = demoAuthDtos.Select(demoAuthDto => new DemoAuthKey
            {
                UserId = demoAuthDto.UserId,
                AuthKey = demoAuthDto.AuthKey,
                Created = demoAuthDto.Created,
                LastActivity = demoAuthDto.LastActivity
            });

            await Context.DemoAuthKeys.AddRangeAsync(demoAuthKeys);
            await Context.SaveChangesAsync();

            var dtos = demoAuthKeys.Select(dak => dak.ToDto());

            return new OkObjectResult(dtos);
        }

        [HttpPut]
        [Route("api/demos-auth")]
        public async Task<IActionResult> UpdateDemoAuthKeys()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<DemoAuthDto>? demoAuthDtos;
            try
            {
                demoAuthDtos = JsonConvert.DeserializeObject<List<DemoAuthDto>>(requestBody);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            if (demoAuthDtos == null || demoAuthDtos.Count == 0)
                return new BadRequestResult();

            var userIds = demoAuthDtos.Select(dak => dak.UserId).ToArray();

            var demoAuthKeys = await Context.DemoAuthKeys.Where(dak => userIds.Contains(dak.UserId)).ToListAsync();
            foreach (var demoAuthDto in demoAuthDtos)
            {
                var demoAuthKey = demoAuthKeys.SingleOrDefault(dak => dak.UserId == demoAuthDto.UserId);

                if (demoAuthKey == null)
                    return new BadRequestResult();

                demoAuthKey.AuthKey = demoAuthDto.AuthKey;
                demoAuthKey.LastActivity = DateTime.UtcNow;
            }

            await Context.SaveChangesAsync();

            var dtos = demoAuthKeys.Select(dak => dak.ToDto());

            return new OkObjectResult(dtos);
        }
    }
}
