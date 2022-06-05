using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class UserProfileController : Controller, IUserProfileApi
    {
        private readonly PortalDbContext context;
        private readonly IMapper mapper;

        public UserProfileController(
            PortalDbContext context,
            IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [Route("api/user-profile/{userProfileId}")]
        public async Task<IActionResult> GetUserProfile(Guid userProfileId)
        {
            var response = await ((IUserProfileApi)this).GetUserProfile(userProfileId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<UserProfileDto>> IUserProfileApi.GetUserProfile(Guid userProfileId)
        {
            var userProfile = await context.UserProfiles
                .Include(up => up.UserProfileClaims)
                .SingleOrDefaultAsync(up => up.Id == userProfileId);

            if (userProfile == null)
                return new ApiResponseDto<UserProfileDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<UserProfileDto>(userProfile);

            return new ApiResponseDto<UserProfileDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("api/user-profile/by-identity-id/{identityId}")]
        public async Task<IActionResult> GetUserProfileByIdentityId(string identityId)
        {
            var response = await ((IUserProfileApi)this).GetUserProfileByIdentityId(identityId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<UserProfileDto>> IUserProfileApi.GetUserProfileByIdentityId(string identityId)
        {
            var userProfile = await context.UserProfiles
                .Include(up => up.UserProfileClaims)
                .SingleOrDefaultAsync(up => up.IdentityOid == identityId);

            if (userProfile == null)
                return new ApiResponseDto<UserProfileDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<UserProfileDto>(userProfile);

            return new ApiResponseDto<UserProfileDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("api/user-profile/by-xtremeidiots-id/{xtremeidiotsId}")]
        public async Task<IActionResult> GetUserProfileByXtremeIdiotsId(string xtremeIdiotsId)
        {
            var response = await ((IUserProfileApi)this).GetUserProfileByXtremeIdiotsId(xtremeIdiotsId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<UserProfileDto>> IUserProfileApi.GetUserProfileByXtremeIdiotsId(string xtremeIdiotsId)
        {
            var userProfile = await context.UserProfiles
                .Include(up => up.UserProfileClaims)
                .SingleOrDefaultAsync(up => up.XtremeIdiotsForumId == xtremeIdiotsId);

            if (userProfile == null)
                return new ApiResponseDto<UserProfileDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<UserProfileDto>(userProfile);

            return new ApiResponseDto<UserProfileDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("api/user-profile")]
        public async Task<IActionResult> GetUserProfiles(string? filterString, int? skipEntries, int? takeEntries, UserProfilesOrder? order)
        {
            if (!skipEntries.HasValue)
                skipEntries = 0;

            if (!takeEntries.HasValue)
                takeEntries = 20;

            var response = await ((IUserProfileApi)this).GetUserProfiles(filterString, skipEntries.Value, takeEntries.Value, order);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<UserProfileCollectionDto>> IUserProfileApi.GetUserProfiles(string? filterString, int skipEntries, int takeEntries, UserProfilesOrder? order)
        {
            var query = context.UserProfiles.Include(up => up.UserProfileClaims).AsQueryable();
            query = ApplyFilter(query, filterString);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, filterString);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
            var results = await query.ToListAsync();

            var entries = results.Select(up => mapper.Map<UserProfileDto>(up)).ToList();

            var result = new UserProfileCollectionDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new ApiResponseDto<UserProfileCollectionDto>(HttpStatusCode.OK, result);
        }

        Task<ApiResponseDto> IUserProfileApi.CreateUserProfile(CreateUserProfileDto createUserProfileDto)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("api/user-profile")]
        public async Task<IActionResult> CreateUserProfiles()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateUserProfileDto>? createUserProfileDto;
            try
            {
                createUserProfileDto = JsonConvert.DeserializeObject<List<CreateUserProfileDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createUserProfileDto == null || !createUserProfileDto.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IUserProfileApi)this).CreateUserProfiles(createUserProfileDto);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IUserProfileApi.CreateUserProfiles(List<CreateUserProfileDto> createUserProfileDtos)
        {
            var userProfiles = createUserProfileDtos.Select(up => mapper.Map<UserProfile>(up)).ToList();

            await context.UserProfiles.AddRangeAsync(userProfiles);
            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        Task<ApiResponseDto> IUserProfileApi.UpdateUserProfile(EditUserProfileDto editUserProfileDto)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [Route("api/user-profile")]
        public async Task<IActionResult> UpdateUserProfiles()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<EditUserProfileDto>? editUserProfileDto;
            try
            {
                editUserProfileDto = JsonConvert.DeserializeObject<List<EditUserProfileDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (editUserProfileDto == null || !editUserProfileDto.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IUserProfileApi)this).UpdateUserProfiles(editUserProfileDto);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IUserProfileApi.UpdateUserProfiles(List<EditUserProfileDto> editUserProfileDtos)
        {
            foreach (var editUserProfileDto in editUserProfileDtos)
            {
                var userProfile = await context.UserProfiles.SingleAsync(up => up.Id == editUserProfileDto.Id);
                mapper.Map(editUserProfileDto, userProfile);
            }

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpPatch]
        [Route("api/user-profile/{userProfileId}/claims")]
        public async Task<IActionResult> CreateUserProfileClaim(Guid userProfileId)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateUserProfileClaimDto>? createUserProfileClaimDtos;
            try
            {
                createUserProfileClaimDtos = JsonConvert.DeserializeObject<List<CreateUserProfileClaimDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createUserProfileClaimDtos == null)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null").ToHttpResult();

            var response = await ((IUserProfileApi)this).CreateUserProfileClaim(userProfileId, createUserProfileClaimDtos);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IUserProfileApi.CreateUserProfileClaim(Guid userProfileId, List<CreateUserProfileClaimDto> createUserProfileClaimDto)
        {
            var userProfileClaims = createUserProfileClaimDto.Select(upc => mapper.Map<UserProfileClaim>(upc)).ToList();

            await context.UserProfileClaims.AddRangeAsync(userProfileClaims);
            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("api/user-profile/{userProfileId}/claims")]
        public async Task<IActionResult> SetUserProfileClaims(Guid userProfileId)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateUserProfileClaimDto>? createUserProfileClaimDtos;
            try
            {
                createUserProfileClaimDtos = JsonConvert.DeserializeObject<List<CreateUserProfileClaimDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createUserProfileClaimDtos == null)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null").ToHttpResult();

            var response = await ((IUserProfileApi)this).SetUserProfileClaims(userProfileId, createUserProfileClaimDtos);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IUserProfileApi.SetUserProfileClaims(Guid userProfileId, List<CreateUserProfileClaimDto> createUserProfileClaimDto)
        {
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.UserProfileClaims)}] WHERE [UserProfileId] = '{userProfileId}'");

            var userProfileClaims = createUserProfileClaimDto.Select(upc => mapper.Map<UserProfileClaim>(upc)).ToList();

            await context.UserProfileClaims.AddRangeAsync(userProfileClaims);
            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("api/user-profile/{userProfileId}/claims/{userProfileClaimId}")]
        public async Task<IActionResult> DeleteUserProfileClaim(Guid userProfileId, Guid userProfileClaimId)
        {
            var response = await ((IUserProfileApi)this).DeleteUserProfileClaim(userProfileId, userProfileClaimId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IUserProfileApi.DeleteUserProfileClaim(Guid userProfileId, Guid userProfileClaimId)
        {
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.UserProfileClaims)}] WHERE [UserProfileId] = '{userProfileId}' AND [Id] = '{userProfileClaimId}'");
            return new ApiResponseDto(HttpStatusCode.OK);
        }

        private IQueryable<UserProfile> ApplyFilter(IQueryable<UserProfile> query, string? filterString)
        {
            if (!string.IsNullOrEmpty(filterString))
                query = query.Where(up => up.DisplayName.Contains(filterString) || up.Email.Contains(filterString)).AsQueryable();

            return query;
        }

        private IQueryable<UserProfile> ApplyOrderAndLimits(IQueryable<UserProfile> query, int skipEntries, int takeEntries, UserProfilesOrder? order)
        {
            query = query.Skip(skipEntries).AsQueryable();
            query = query.Take(takeEntries).AsQueryable();

            if (order.HasValue)
            {
                switch (order)
                {
                    case UserProfilesOrder.DisplayNameAsc:
                        query = query.OrderBy(up => up.DisplayName).AsQueryable();
                        break;
                    case UserProfilesOrder.DisplayNameDesc:
                        query = query.OrderByDescending(up => up.DisplayName).AsQueryable();
                        break;
                }
            }

            return query;
        }
    }
}
