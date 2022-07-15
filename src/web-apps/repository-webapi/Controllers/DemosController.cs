using AutoMapper;

using Azure.Storage.Blobs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net;

using XtremeIdiots.CodDemos.Models;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class DemosController : Controller, IDemosApi
    {
        private readonly PortalDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public DemosController(
            PortalDbContext context,
            IMapper mapper,
            IConfiguration configuration)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet]
        [Route("demos/{demoId}")]
        public async Task<IActionResult> GetDemo(Guid demoId)
        {
            var response = await ((IDemosApi)this).GetDemo(demoId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<DemoDto>> IDemosApi.GetDemo(Guid demoId)
        {
            var demo = await context.Demos.Include(d => d.UserProfile)
                .SingleOrDefaultAsync(d => d.DemoId == demoId);

            if (demo == null)
                return new ApiResponseDto<DemoDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<DemoDto>(demo);

            return new ApiResponseDto<DemoDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("demos")]
        public async Task<IActionResult> GetDemos(string? gameTypes, string? userId, string? filterString, int? skipEntries, int? takeEntries, DemoOrder? order)
        {
            if (!skipEntries.HasValue)
                skipEntries = 0;

            if (!takeEntries.HasValue)
                takeEntries = 20;

            var demos = context.Demos.Include(d => d.UserProfile).AsQueryable();

            GameType[]? gameTypesFilter = null;
            if (!string.IsNullOrWhiteSpace(gameTypes))
            {
                var split = gameTypes.Split(",");
                gameTypesFilter = split.Select(gt => Enum.Parse<GameType>(gt)).ToArray();
            }

            var response = await ((IDemosApi)this).GetDemos(gameTypesFilter, userId, filterString, skipEntries.Value, takeEntries.Value, order);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<DemosCollectionDto>> IDemosApi.GetDemos(GameType[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, DemoOrder? order)
        {
            var query = context.Demos.Include(d => d.UserProfile).AsQueryable();
            query = ApplyFilter(query, gameTypes, null, null);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, gameTypes, userId, filterString);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
            var results = await query.ToListAsync();

            var entries = results.Select(d => mapper.Map<DemoDto>(d)).ToList();

            var result = new DemosCollectionDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new ApiResponseDto<DemosCollectionDto>(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("demos")]
        public async Task<IActionResult> CreateDemo()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            CreateDemoDto? createDemoDto;
            try
            {
                createDemoDto = JsonConvert.DeserializeObject<CreateDemoDto>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createDemoDto == null)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null").ToHttpResult();

            var response = await ((IDemosApi)this).CreateDemo(createDemoDto);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<DemoDto>> IDemosApi.CreateDemo(CreateDemoDto createDemoDto)
        {
            var demo = new Demo
            {
                DemoId = Guid.NewGuid(),
                GameType = createDemoDto.GameType.ToGameTypeInt(),
                UserProfileId = createDemoDto.UserProfileId
            };

            context.Demos.Add(demo);
            await context.SaveChangesAsync();

            var result = mapper.Map<DemoDto>(demo);

            return new ApiResponseDto<DemoDto>(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("demos/{demoId}/file")]
        public async Task<IActionResult> SetDemoFile(Guid demoId)
        {
            if (Request.Form.Files.Count == 0)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request did not contain any files").ToHttpResult();

            var whitelistedExtensions = new List<string> { ".dm_1", ".dm_6" };

            var file = Request.Form.Files.First();
            if (!whitelistedExtensions.Any(ext => file.FileName.EndsWith(ext)))
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Invalid file type extension").ToHttpResult();

            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
                await file.CopyToAsync(stream);

            var response = await ((IDemosApi)this).SetDemoFile(demoId, file.FileName, filePath);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IDemosApi.SetDemoFile(Guid demoId, string fileName, string filePath)
        {
            var demo = context.Demos.SingleOrDefault(d => d.DemoId == demoId);

            if (demo == null)
                return new ApiResponseDto(HttpStatusCode.NotFound);

            var blobKey = $"{Guid.NewGuid()}.{demo.GameType.ToGameType().DemoExtension()}";
            var blobServiceClient = new BlobServiceClient(configuration["appdata_storage_connectionstring"]);
            var containerClient = blobServiceClient.GetBlobContainerClient("demos");
            var blobClient = containerClient.GetBlobClient(blobKey);
            await blobClient.UploadAsync(filePath);

            var localDemo = new LocalDemo(filePath, demo.GameType.ToGameType());

            demo.Title = Path.GetFileNameWithoutExtension(fileName);
            demo.FileName = blobKey;
            demo.Created = localDemo.Created;
            demo.Map = localDemo.Map;
            demo.Mod = localDemo.Mod;
            demo.GameMode = localDemo.GameMode;
            demo.ServerName = localDemo.ServerName;
            demo.FileSize = localDemo.FileSize;
            demo.FileUri = blobClient.Uri.ToString();

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("demos/{demoId}")]
        public async Task<IActionResult> DeleteDemo(Guid demoId)
        {
            var response = await ((IDemosApi)this).DeleteDemo(demoId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IDemosApi.DeleteDemo(Guid demoId)
        {
            var demo = await context.Demos.SingleOrDefaultAsync(d => d.DemoId == demoId);

            if (demo == null)
                return new ApiResponseDto(HttpStatusCode.NotFound);

            context.Remove(demo);

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        private IQueryable<Demo> ApplyFilter(IQueryable<Demo> query, GameType[]? gameTypes, string? userId, string? filterString)
        {
            if (gameTypes != null && gameTypes.Length > 0)
            {
                var gameTypeInts = gameTypes.Select(gt => gt.ToGameTypeInt()).ToArray();
                query = query.Where(d => gameTypeInts.Contains(d.GameType)).AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(d => d.UserProfile.XtremeIdiotsForumId == userId).AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(filterString))
            {
                query = query.Where(d => d.Title.Contains(filterString) || d.UserProfile.DisplayName.Contains(filterString)).AsQueryable();
            }

            return query;
        }

        private IQueryable<Demo> ApplyOrderAndLimits(IQueryable<Demo> query, int skipEntries, int takeEntries, DemoOrder? order)
        {
            switch (order)
            {
                case DemoOrder.GameTypeAsc:
                    query = query.OrderBy(d => d.GameType).AsQueryable();
                    break;
                case DemoOrder.GameTypeDesc:
                    query = query.OrderByDescending(d => d.GameType).AsQueryable();
                    break;
                case DemoOrder.TitleAsc:
                    query = query.OrderBy(d => d.Title).AsQueryable();
                    break;
                case DemoOrder.TitleDesc:
                    query = query.OrderByDescending(d => d.Title).AsQueryable();
                    break;
                case DemoOrder.CreatedAsc:
                    query = query.OrderBy(d => d.Created).AsQueryable();
                    break;
                case DemoOrder.CreatedDesc:
                    query = query.OrderByDescending(d => d.Created).AsQueryable();
                    break;
                case DemoOrder.UploadedByAsc:
                    query = query.OrderBy(d => d.UserProfile.DisplayName).AsQueryable();
                    break;
                case DemoOrder.UploadedByDesc:
                    query = query.OrderByDescending(d => d.UserProfile.DisplayName).AsQueryable();
                    break;
            }

            query = query.Skip(skipEntries).AsQueryable();
            query = query.Take(takeEntries).AsQueryable();

            return query;
        }
    }
}
