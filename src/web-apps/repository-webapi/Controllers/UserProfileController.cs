﻿using Microsoft.AspNetCore.Authorization;
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
        [Route("api/user-profile")]
        public async Task<IActionResult> GetUserProfiles(int? skipEntries, int? takeEntries, string? filterString)
        {
            if (skipEntries == null)
                skipEntries = 0;

            if (takeEntries == null)
                takeEntries = 10;

            var query = Context.UserProfiles.AsQueryable();
            query = ApplyFilter(query, filterString);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, filterString);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, (int)skipEntries, (int)takeEntries);
            var results = await query.ToListAsync();

            var entries = results.Select(up => up.ToDto()).ToList();

            var response = new UserProfileResponseDto
            {
                Skipped = (int)skipEntries,
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new OkObjectResult(response);
        }

        private IQueryable<UserProfile> ApplyFilter(IQueryable<UserProfile> userProfiles, string? filterString)
        {
            if (!string.IsNullOrEmpty(filterString))
                userProfiles = userProfiles.Where(up => up.DisplayName.Contains(filterString) || up.Email.Contains(filterString)).AsQueryable();

            return userProfiles;
        }

        private IQueryable<UserProfile> ApplyOrderAndLimits(IQueryable<UserProfile> userProfiles, int skipEntries, int takeEntries)
        {
            userProfiles = userProfiles.Skip(skipEntries).AsQueryable();
            userProfiles = userProfiles.Take(takeEntries).AsQueryable();

            return userProfiles;
        }

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
                userProfile.DisplayName = userProfileDto.DisplayName;
                userProfile.Title = userProfileDto.Title;
                userProfile.FormattedName = userProfileDto.FormattedName;
                userProfile.PrimaryGroup = userProfileDto.PrimaryGroup;
                userProfile.Email = userProfileDto.Email;
                userProfile.PhotoUrl = userProfileDto.PhotoUrl;
                userProfile.ProfileUrl = userProfileDto.ProfileUrl;
                userProfile.TimeZone = userProfileDto.TimeZone;
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

            await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.UserProfileClaims)}] WHERE [UserProfileId] = '{id}'");

            var userProfileClaims = userProfileClaimDtos.Select(upc => new UserProfileClaim
            {
                UserProfileId = id,
                SystemGenerated = upc.SystemGenerated,
                ClaimType = upc.ClaimType,
                ClaimValue = upc.ClaimValue
            }).ToList();

            await Context.UserProfileClaims.AddRangeAsync(userProfileClaims);
            await Context.SaveChangesAsync();

            var result = userProfileClaims.Select(upc => upc.ToDto());

            return new OkObjectResult(result);
        }
    }
}
