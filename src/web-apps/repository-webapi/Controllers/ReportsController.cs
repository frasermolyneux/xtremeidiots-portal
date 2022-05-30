using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger<ReportsController> logger;
        private readonly PortalDbContext context;

        public ReportsController(
            ILogger<ReportsController> logger,
            PortalDbContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        [Route("api/reports")]
        public async Task<IActionResult> GetReports(GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filterType, int? skipEntries, int? takeEntries, ReportsOrder? order)
        {
            if (!skipEntries.HasValue)
                skipEntries = 0;

            if (!takeEntries.HasValue)
                takeEntries = 20;

            if (cutoff.HasValue && cutoff.Value < DateTime.UtcNow.AddHours(-48))
                cutoff = DateTime.UtcNow.AddHours(-48);

            var query = context.Reports.Include(r => r.UserProfile).Include(r => r.AdminUserProfile).AsQueryable();
            query = ApplyFilter(query, gameType, null, null, null);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, gameType, serverId, cutoff, filterType);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, (int)skipEntries, (int)takeEntries, order);
            var results = await query.ToListAsync();

            var entries = results.Select(rp => new ReportDtoWrapper(rp)).ToList();

            var response = new CollectionResponseDto<ReportDtoWrapper>
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new OkObjectResult(response);
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (createReportDtos == null || createReportDtos.Count == 0)
                return new BadRequestResult();

            var reports = new List<Report>();

            foreach (var reportDto in createReportDtos)
            {
                var player = await context.Players.SingleOrDefaultAsync(p => p.PlayerId == reportDto.PlayerId);

                if (player == null)
                    return BadRequest("Could not find player with specified player id");

                var userProfile = await context.UserProfiles.SingleOrDefaultAsync(up => up.Id == reportDto.UserProfileId);

                if (userProfile == null)
                    return BadRequest("Could not user profile with specified user profile id");

                var report = new Report
                {
                    PlayerId = player.PlayerId,
                    UserProfileId = userProfile.Id,
                    ServerId = reportDto.ServerId,
                    GameType = player.GameType,
                    Comments = reportDto.Comments,
                    Timestamp = DateTime.UtcNow
                };

                reports.Add(report);
            }

            await context.Reports.AddRangeAsync(reports);
            await context.SaveChangesAsync();

            var entries = reports.Select(rp => new ReportDtoWrapper(rp)).ToList();

            var response = new CollectionResponseDto<ReportDtoWrapper>
            {
                TotalRecords = reports.Count,
                FilteredRecords = reports.Count,
                Entries = entries
            };

            return new OkObjectResult(response);
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (closeReportDto == null)
                return new BadRequestResult();

            var report = await context.Reports.SingleOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return NotFound();

            var userProfile = await context.UserProfiles.SingleOrDefaultAsync(up => up.Id == closeReportDto.AdminUserProfileId);

            if (userProfile == null)
                return BadRequest("Could not user profile with specified user profile id");

            report.UserProfileId = userProfile.Id;
            report.AdminClosingComments = closeReportDto.AdminClosingComments;
            report.Closed = true;
            report.ClosedTimestamp = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var response = new ReportDtoWrapper(report);

            return new OkObjectResult(response);
        }

        private static IQueryable<Report> ApplyFilter(IQueryable<Report> query, GameType? gameType, Guid? serverId, DateTime? cutoff, ReportsFilter? filterType)
        {
            if (gameType.HasValue)
                query = query.Where(r => r.GameType == ((GameType)gameType).ToGameTypeInt()).AsQueryable();

            if (serverId.HasValue)
                query = query.Where(r => r.ServerId == serverId).AsQueryable();

            if (cutoff.HasValue)
                query = query.Where(r => r.Timestamp > cutoff).AsQueryable();

            if (filterType.HasValue)
            {
                switch (filterType)
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

        public class ReportDtoWrapper : ReportDto
        {
            public ReportDtoWrapper(Report report)
            {
                Id = report.Id;
                PlayerId = report.PlayerId;
                UserProfileId = report.UserProfileId;
                ServerId = report.ServerId;
                GameType = report.GameType.ToGameType();
                Comments = report.Comments;
                Timestamp = report.Timestamp;
                AdminUserProfileId = report.AdminUserProfileId;
                AdminClosingComments = report.AdminClosingComments;
                Closed = report.Closed;
                ClosedTimestamp = report.ClosedTimestamp;

                if (report.UserProfile != null)
                    UserProfile = report.UserProfile.ToDto();

                if (report.AdminUserProfile != null)
                    AdminUserProfile = report.AdminUserProfile.ToDto();
            }
        }
    }
}
