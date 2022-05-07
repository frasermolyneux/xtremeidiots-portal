using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class DemosController : Controller
    {
        public DemosController(PortalDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PortalDbContext Context { get; }

        [HttpGet]
        [Route("api/demos")]
        public async Task<IActionResult> GetDemos(string? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, DemoOrder? order)
        {
            var demos = Context.Demoes.Include(d => d.User).AsQueryable();

            int[] filterByGameTypes = { };
            if (!string.IsNullOrWhiteSpace(gameTypes))
            {
                var split = gameTypes.Split(",");

                filterByGameTypes = split.Select(gt => Enum.Parse<GameType>(gt)).Select(gt => gt.ToGameTypeInt()).ToArray();
            }

            if (order == null)
                order = DemoOrder.DateDesc;


            var query = Context.Demoes.Include(d => d.User).AsQueryable();
            query = ApplySearchFilter(query, Array.Empty<int>(), null, null);
            var totalCount = await query.CountAsync();

            query = ApplySearchFilter(query, filterByGameTypes, userId, filterString);
            var filteredCount = await query.CountAsync();

            query = ApplySearchOrderAndLimits(query, (DemoOrder)order, skipEntries, takeEntries);
            var searchResults = await query.ToListAsync();

            var entries = searchResults.Select(demo => new DemoDto()
            {
                DemoId = demo.DemoId,
                Game = demo.Game.ToGameType(),
                Name = demo.Name,
                FileName = demo.FileName,
                Date = demo.Date,
                Map = demo.Map,
                Mod = demo.Mod,
                GameType = demo.GameType,
                Server = demo.Server,
                Size = demo.Size,

                UserId = demo.User.XtremeIdiotsId,
                UploadedBy = demo.User.UserName
            }).ToList();

            var response = new DemosSearchResponseDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new OkObjectResult(response);
        }

        private IQueryable<Demo> ApplySearchFilter(IQueryable<Demo> query, int[] filterByGameTypes, string? userId, string? filterString)
        {
            if (userId != null && filterByGameTypes.Count() != 0)
                query = query.Where(d => filterByGameTypes.Contains(d.Game) && d.UserId == userId).AsQueryable();
            else if (filterByGameTypes.Count() != 0)
                query = query.Where(d => filterByGameTypes.Contains(d.Game)).AsQueryable();
            else if (userId != null)
                query = query.Where(d => d.UserId == userId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterString))
                query = query.Where(d => d.Name.Contains(filterString) || d.User.UserName.Contains(filterString)).AsQueryable();

            return query;
        }

        private IQueryable<Demo> ApplySearchOrderAndLimits(IQueryable<Demo> query, DemoOrder order, int skipEntries, int takeEntries)
        {
            switch (order)
            {
                case DemoOrder.GameTypeAsc:
                    query = query.OrderBy(d => d.Game).AsQueryable();
                    break;
                case DemoOrder.GameTypeDesc:
                    query = query.OrderByDescending(d => d.Game).AsQueryable();
                    break;
                case DemoOrder.NameAsc:
                    query = query.OrderBy(d => d.Name).AsQueryable();
                    break;
                case DemoOrder.NameDesc:
                    query = query.OrderByDescending(d => d.Name).AsQueryable();
                    break;
                case DemoOrder.DateAsc:
                    query = query.OrderBy(d => d.Date).AsQueryable();
                    break;
                case DemoOrder.DateDesc:
                    query = query.OrderByDescending(d => d.Date).AsQueryable();
                    break;
                case DemoOrder.UploadedByAsc:
                    query = query.OrderBy(d => d.User.UserName).AsQueryable();
                    break;
                case DemoOrder.UploadedByDesc:
                    query = query.OrderByDescending(d => d.User.UserName).AsQueryable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            query = query.Skip(skipEntries).AsQueryable();

            if (takeEntries != 0) query = query.Take(takeEntries).AsQueryable();

            return query;
        }

        [HttpPost]
        [Route("api/demos")]
        public async Task<IActionResult> CreateDemo()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            DemoDto demoDto;
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                demoDto = JsonConvert.DeserializeObject<DemoDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            if (demoDto == null) return new BadRequestResult();

            var demo = new Demo
            {
                DemoId = Guid.NewGuid(),
                Game = demoDto.Game.ToGameTypeInt(),
                Name = demoDto.Name,
                FileName = demoDto.FileName,
                Date = demoDto.Date,
                Map = demoDto.Map,
                Mod = demoDto.Mod,
                GameType = demoDto.GameType,
                Server = demoDto.Server,
                Size = demoDto.Size,
                User = await Context.AspNetUsers.SingleAsync(u => u.XtremeIdiotsId == demoDto.UserId)
            };

            Context.Demoes.Add(demo);
            await Context.SaveChangesAsync();

            var result = new DemoDto
            {
                DemoId = demo.DemoId,
                Game = demo.Game.ToGameType(),
                Name = demo.Name,
                FileName = demo.FileName,
                Date = demo.Date,
                Map = demo.Map,
                Mod = demo.Mod,
                GameType = demo.GameType,
                Server = demo.Server,
                Size = demo.Size,

                UserId = demo.User.XtremeIdiotsId,
                UploadedBy = demo.User.UserName
            };

            return new OkObjectResult(result);
        }

        [HttpGet]
        [Route("api/demos/{demoId}")]
        public async Task<IActionResult> GetDemo(Guid? demoId)
        {
            var demo = await Context.Demoes.Include(d => d.User).SingleOrDefaultAsync(d => d.DemoId == demoId);

            var result = new DemoDto
            {
                DemoId = demo.DemoId,
                Game = demo.Game.ToGameType(),
                Name = demo.Name,
                FileName = demo.FileName,
                Date = demo.Date,
                Map = demo.Map,
                Mod = demo.Mod,
                GameType = demo.GameType,
                Server = demo.Server,
                Size = demo.Size,

                UserId = demo.User.XtremeIdiotsId,
                UploadedBy = demo.User.UserName
            };

            return new OkObjectResult(result);
        }

        [HttpDelete]
        [Route("api/demos/{demoId}")]
        public async Task<IActionResult> DeleteDemo(Guid? demoId)
        {
            var demo = await Context.Demoes.SingleOrDefaultAsync(d => d.DemoId == demoId);

            if (demo == null)
                throw new NullReferenceException(nameof(demo));

            Context.Remove(demo);
            await Context.SaveChangesAsync();

            return new OkResult();
        }
    }
}
