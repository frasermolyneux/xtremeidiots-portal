using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Demos.Dto;
using XI.Portal.Demos.Extensions;
using XI.Portal.Demos.Interfaces;
using XI.Portal.Demos.Models;

namespace XI.Portal.Demos.Repository
{
    public class DemosRepository : IDemosRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IDemosRepositoryOptions _options;

        public DemosRepository(IDemosRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetDemosCount(DemosFilterModel filterModel)
        {
            return await _legacyContext.Demoes.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<DemoDto>> GetDemos(DemosFilterModel filterModel)
        {
            var demos = await _legacyContext.Demoes.ApplyFilter(filterModel).ToListAsync();

            var models = demos.Select(d => d.ToDto()).ToList();

            return models;
        }

        public async Task<DemoDto> GetDemo(Guid demoId)
        {
            var demo = await _legacyContext.Demoes
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.DemoId == demoId);

            return demo?.ToDto();
        }

        public async Task CreateDemo(DemoDto demoDto, string filePath)
        {
            var blobKey = demoDto.FileName;

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);

            var data = File.ReadAllBytes(filePath);
            await blobClient.UploadAsync(new MemoryStream(data));

            var demo = new Demoes
            {
                DemoId = Guid.NewGuid(),
                Game = demoDto.Game,
                Name = demoDto.Name,
                FileName = demoDto.FileName,
                Date = demoDto.Date,
                Map = demoDto.Map,
                Mod = demoDto.Mod,
                GameType = demoDto.GameType,
                Server = demoDto.Server,
                Size = demoDto.Size,
                User = await _legacyContext.AspNetUsers.SingleAsync(u => u.XtremeIdiotsId == demoDto.UserId)
            };

            _legacyContext.Demoes.Add(demo);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task DeleteDemo(Guid demoId)
        {
            var demo = await _legacyContext.Demoes
                .SingleOrDefaultAsync(d => d.DemoId == demoId);

            if (demo == null)
                throw new NullReferenceException(nameof(demo));

            _legacyContext.Remove(demo);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<Uri> GetDemoUrl(Guid demoId)
        {
            var demoDto = await GetDemo(demoId);

            if (demoDto == null)
                throw new NullReferenceException(nameof(demoDto));

            var blobKey = demoDto.FileName;

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            var blobClient = containerClient.GetBlobClient(blobKey);

            return blobClient.Uri;
        }
    }
}