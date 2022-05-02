using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class DemosController : Controller
    {
        public DemosController(LegacyPortalContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public LegacyPortalContext Context { get; }

        [HttpGet]
        [Route("api/demos")]
        public async Task<IActionResult> GetDemos(string? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, string? order)
        {
            var demos = Context.Demoes.Include(d => d.User).AsQueryable();

            GameType[] filterByGameTypes = { };
            if (!string.IsNullOrWhiteSpace(gameTypes))
            {
                var split = gameTypes.Split(",");

                filterByGameTypes = split.Select(gt => Enum.Parse<GameType>(gt)).ToArray();
            }

            if (string.IsNullOrWhiteSpace(order))
                order = "DateDesc";

            if (userId != null && filterByGameTypes.Count() != 0)
                demos = demos.Where(d => filterByGameTypes.Contains(d.Game) && d.UserId == userId).AsQueryable();
            else if (filterByGameTypes.Count() != 0)
                demos = demos.Where(d => filterByGameTypes.Contains(d.Game)).AsQueryable();
            else if (userId != null)
                demos = demos.Where(d => d.UserId == userId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterString))
                demos = demos.Where(d => d.Name.Contains(filterString) || d.User.UserName.Contains(filterString)).AsQueryable();

            switch (order)
            {
                case "GameTypeAsc":
                    demos = demos.OrderBy(d => d.Game).AsQueryable();
                    break;
                case "GameTypeDesc":
                    demos = demos.OrderByDescending(d => d.Game).AsQueryable();
                    break;
                case "NameAsc":
                    demos = demos.OrderBy(d => d.Name).AsQueryable();
                    break;
                case "NameDesc":
                    demos = demos.OrderByDescending(d => d.Name).AsQueryable();
                    break;
                case "DateAsc":
                    demos = demos.OrderBy(d => d.Date).AsQueryable();
                    break;
                case "DateDesc":
                    demos = demos.OrderByDescending(d => d.Date).AsQueryable();
                    break;
                case "UploadedByAsc":
                    demos = demos.OrderBy(d => d.User.UserName).AsQueryable();
                    break;
                case "UploadedByDesc":
                    demos = demos.OrderByDescending(d => d.User.UserName).AsQueryable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            demos = demos.Skip(skipEntries).AsQueryable();

            if (takeEntries != 0) demos = demos.Take(takeEntries).AsQueryable();

            var results = await demos.ToListAsync();

            var result = results.Select(demo => new DemoDto
            {
                DemoId = demo.DemoId,
                Game = demo.Game.ToString(),
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

            return new OkObjectResult(result);
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

            var demo = new Demoes
            {
                DemoId = Guid.NewGuid(),
                Game = Enum.Parse<GameType>(demoDto.Game),
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
                Game = demo.Game.ToString(),
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
                Game = demo.Game.ToString(),
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
