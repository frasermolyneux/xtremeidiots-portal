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
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class ReportsController : ControllerBase, IReportsApi
    {
        private readonly PortalDbContext context;
        private readonly IMapper mapper;

        public ReportsController(
            PortalDbContext context,
            IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [Route("api/reports/{reportId}")]
        public async Task<IActionResult> GetReport(Guid reportId)
        {
            var response = await ((IReportsApi)this).GetReport(reportId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<ReportDto>> IReportsApi.GetReport(Guid reportId)
        {
            var report = await context.Reports.SingleOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return new ApiResponseDto<ReportDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<ReportDto>(report);

            return new ApiResponseDto<ReportDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("api/reports")]
        public async Task<IActionResult> GetReports(GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filter, int? skipEntries, int? takeEntries, ReportsOrder? order)
        {
            if (!skipEntries.HasValue)
                skipEntries = 0;

            if (!takeEntries.HasValue)
                takeEntries = 20;

            if (cutoff.HasValue && cutoff.Value < DateTime.UtcNow.AddDays(-14))
                cutoff = DateTime.UtcNow.AddDays(-14);

            var response = await ((IReportsApi)this).GetReports(gameType, serverId, cutoff, filter, skipEntries.Value, takeEntries.Value, order);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<ReportsCollectionDto>> IReportsApi.GetReports(GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filter, int skipEntries, int takeEntries, ReportsOrder? order)
        {
            var query = context.Reports.Include(r => r.UserProfile).Include(r => r.AdminUserProfile).AsQueryable();
            query = ApplyFilter(query, gameType, null, null, null);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, gameType, serverId, cutoff, filter);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
            var results = await query.ToListAsync();

            var entries = results.Select(r => mapper.Map<ReportDto>(r)).ToList();

            var result = new ReportsCollectionDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new ApiResponseDto<ReportsCollectionDto>(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("api/reports")]
        public async Task<IActionResult> CreateReports()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateReportDto>? createReportDtos;
            try
            {
                createReportDtos = JsonConvert.DeserializeObject<List<CreateReportDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createReportDtos == null || !createReportDtos.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IReportsApi)this).CreateReports(createReportDtos);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IReportsApi.CreateReports(List<CreateReportDto> createReportDtos)
        {
            var reports = createReportDtos.Select(r => mapper.Map<Report>(r)).ToList();

            foreach (var report in reports)
            {
                var gameServer = context.GameServers.Single(gs => gs.ServerId == report.ServerId);
                report.GameType = gameServer.GameType;
            }

            await context.Reports.AddRangeAsync(reports);
            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpPatch]
        [Route("api/reports/{reportId}/close")]
        public async Task<IActionResult> CloseReport(Guid reportId)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            CloseReportDto? closeReportDto;
            try
            {
                closeReportDto = JsonConvert.DeserializeObject<CloseReportDto>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (closeReportDto == null)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null").ToHttpResult();

            var response = await ((IReportsApi)this).CloseReport(reportId, closeReportDto);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IReportsApi.CloseReport(Guid reportId, CloseReportDto closeReportDto)
        {
            var report = await context.Reports.SingleOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return new ApiResponseDto(HttpStatusCode.NotFound);

            var userProfile = await context.UserProfiles.SingleOrDefaultAsync(up => up.Id == closeReportDto.AdminUserProfileId);

            if (userProfile == null)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not user profile with specified user profile id");

            mapper.Map(closeReportDto, report);

            report.Closed = true;
            report.ClosedTimestamp = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        private static IQueryable<Report> ApplyFilter(IQueryable<Report> query, GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filter)
        {
            if (gameType.HasValue)
                query = query.Where(r => r.GameType == ((GameType)gameType).ToGameTypeInt()).AsQueryable();

            if (serverId.HasValue)
                query = query.Where(r => r.ServerId == serverId).AsQueryable();

            if (cutoff.HasValue)
                query = query.Where(r => r.Timestamp > cutoff).AsQueryable();

            if (filter.HasValue)
            {
                switch (filter)
                {
                    case ReportsFilter.OpenReports:
                        query = query.Where(r => !r.Closed).AsQueryable();
                        break;
                    case ReportsFilter.ClosedReports:
                        query = query.Where(r => r.Closed).AsQueryable();
                        break;
                }
            }

            return query;
        }

        private static IQueryable<Report> ApplyOrderAndLimits(IQueryable<Report> query, int skipEntries, int takeEntries, ReportsOrder? order)
        {
            query = query.Skip(skipEntries).AsQueryable();
            query = query.Take(takeEntries).AsQueryable();

            if (order.HasValue)
            {
                switch (order)
                {
                    case ReportsOrder.TimestampAsc:
                        query = query.OrderBy(r => r.Timestamp).AsQueryable();
                        break;
                    case ReportsOrder.TimestampDesc:
                        query = query.OrderByDescending(r => r.Timestamp).AsQueryable();
                        break;
                }
            }

            return query;
        }
    }
}
