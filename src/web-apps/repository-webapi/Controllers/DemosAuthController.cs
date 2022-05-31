using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class DemosAuthController : Controller
    {
        private readonly ILogger<DemosAuthController> logger;
        private readonly PortalDbContext context;

        public DemosAuthController(
            ILogger<DemosAuthController> logger,
            PortalDbContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        [Route("api/demos-auth/{userId}")]
        public async Task<IActionResult> GetDemoAuthKeyByUserId(string userId)
        {
            var demoAuthKey = await context.DemoAuthKeys.SingleOrDefaultAsync(dak => dak.UserId == userId);

            if (demoAuthKey == null)
                return NotFound();

            var dto = demoAuthKey.ToDto();

            return new OkObjectResult(dto);
        }

        [HttpGet]
        [Route("api/demos-auth/by-auth-key/{authKey}")]
        public async Task<IActionResult> GetDemoAuthKeyByAuthKey(string authKey)
        {
            var demoAuthKey = await context.DemoAuthKeys.SingleOrDefaultAsync(dak => dak.AuthKey == authKey);

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
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
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

            await context.DemoAuthKeys.AddRangeAsync(demoAuthKeys);
            await context.SaveChangesAsync();

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
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (demoAuthDtos == null || demoAuthDtos.Count == 0)
                return new BadRequestResult();

            var userIds = demoAuthDtos.Select(dak => dak.UserId).ToArray();

            var demoAuthKeys = await context.DemoAuthKeys.Where(dak => userIds.Contains(dak.UserId)).ToListAsync();
            foreach (var demoAuthDto in demoAuthDtos)
            {
                var demoAuthKey = demoAuthKeys.SingleOrDefault(dak => dak.UserId == demoAuthDto.UserId);

                if (demoAuthKey == null)
                    return new BadRequestResult();

                demoAuthKey.AuthKey = demoAuthDto.AuthKey;
                demoAuthKey.LastActivity = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            var dtos = demoAuthKeys.Select(dak => dak.ToDto());

            return new OkObjectResult(dtos);
        }
    }
}
