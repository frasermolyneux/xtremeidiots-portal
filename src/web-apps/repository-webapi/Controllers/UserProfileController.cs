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
    public class UserProfileController : Controller
    {
        public UserProfileController(PortalDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PortalDbContext Context { get; }

        [HttpGet]
        [Route("api/user-profile/{id}")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
            var userProfile = await Context.UserProfiles
                .SingleOrDefaultAsync(up => up.Id == id);

            if (userProfile == null)
                return NotFound();

            return new OkObjectResult(userProfile.ToDto());
        }

        [HttpGet]
        [Route("api/user-profile/by-identity-id/{identityId}")]
        public async Task<IActionResult> GetUserProfileByIdentityId(string identityId)
        {
            var userProfile = await Context.UserProfiles
                .SingleOrDefaultAsync(up => up.IdentityOid == identityId);

            if (userProfile == null)
                return NotFound();

            return new OkObjectResult(userProfile.ToDto());
        }

        [HttpGet]
        [Route("api/user-profile/by-xtremeidiots-id/{xtremeidiotsId}")]
        public async Task<IActionResult> GetUserProfileByXtremeIdiotsId(string xtremeIdiotsId)
        {
            var userProfile = await Context.UserProfiles
                .SingleOrDefaultAsync(up => up.XtremeIdiotsForumId == xtremeIdiotsId);

            if (userProfile == null)
                return NotFound();

            return new OkObjectResult(userProfile.ToDto());
        }

        [HttpPost]
        [Route("api/user-profile")]
        public async Task<IActionResult> CreateUserProfiles()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<UserProfileDto>? userProfileDtos;
            try
            {
                userProfileDtos = JsonConvert.DeserializeObject<List<UserProfileDto>>(requestBody);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            if (userProfileDtos == null || userProfileDtos.Count == 0)
                return new BadRequestResult();

            var userProfiles = userProfileDtos.Select(up => new UserProfile
            {
                IdentityOid = up.IdentityOid,
                XtremeIdiotsForumId = up.XtremeIdiotsForumId
            });

            await Context.UserProfiles.AddRangeAsync(userProfiles);
            await Context.SaveChangesAsync();

            var result = userProfiles.Select(up => up.ToDto());

            return new OkObjectResult(result);
        }

        [HttpPut]
        [Route("api/user-profile")]
        public async Task<IActionResult> UpdateUserProfiles()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<UserProfileDto>? userProfileDtos;
            try
            {
                userProfileDtos = JsonConvert.DeserializeObject<List<UserProfileDto>>(requestBody);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            if (userProfileDtos == null || userProfileDtos.Count == 0)
                return new BadRequestResult();

            var userProfileIds = userProfileDtos.Select(up => up.Id).ToArray();

            var userProfiles = await Context.UserProfiles.Where(up => userProfileIds.Contains(up.Id)).ToListAsync();
            foreach (var userProfileDto in userProfileDtos)
            {
                var userProfile = userProfiles.SingleOrDefault(up => up.Id == userProfileDto.Id);

                if (userProfile == null)
                    return new BadRequestResult();

                userProfile.IdentityOid = userProfileDto.IdentityOid;
                userProfile.XtremeIdiotsForumId = userProfileDto.XtremeIdiotsForumId;
            }

            await Context.SaveChangesAsync();

            var result = userProfiles.Select(up => up.ToDto());

            return new OkObjectResult(result);
        }

        [HttpGet]
        [Route("api/user-profile/{id}/claims")]
        public async Task<IActionResult> GetUserProfileClaims(Guid id)
        {
            var userProfileClaims = await Context.UserProfileClaims
                .Where(upc => upc.UserProfileId == id).ToListAsync();

            var result = userProfileClaims.Select(upc => upc.ToDto());

            return new OkObjectResult(result);
        }

        [HttpPost]
        [Route("api/user-profile/{id}/claims")]
        public async Task<IActionResult> CreateUserProfileClaims(Guid id)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<UserProfileClaimDto>? userProfileClaimDtos;
            try
            {
                userProfileClaimDtos = JsonConvert.DeserializeObject<List<UserProfileClaimDto>>(requestBody);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            if (userProfileClaimDtos == null)
                return new BadRequestResult();

            var userProfile = await Context.UserProfiles.Include(up => up.UserProfileClaims).SingleOrDefaultAsync(up => up.Id == id);

            if (userProfile == null)
                return NotFound();

            userProfile.UserProfileClaims.Clear();

            userProfile.UserProfileClaims = userProfileClaimDtos.Select(upc => new UserProfileClaim
            {
                UserProfileId = userProfile.Id,
                SystemGenerated = upc.SystemGenerated,
                ClaimType = upc.ClaimType,
                ClaimValue = upc.ClaimValue
            }).ToList();

            await Context.SaveChangesAsync();

            var result = userProfile.UserProfileClaims.Select(upc => upc.ToDto());

            return new OkObjectResult(result);
        }
    }
}
