using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Repository
{
    public class BanFilesRepository : IBanFilesRepository
    {
        private readonly IAdminActionsRepository _adminActionsRepository;
        private readonly ILogger<BanFilesRepository> _logger;
        private readonly IBanFilesRepositoryOptions _options;

        public BanFilesRepository(
            ILogger<BanFilesRepository> logger,
            IBanFilesRepositoryOptions options,
            IAdminActionsRepository adminActionsRepository
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
        }

        public async Task RegenerateBanFileForGame(GameType gameType)
        {
            var blobKey = $"{gameType}-bans.txt";

            _logger.LogInformation($"Regenerating ban file for {gameType} using blob key {blobKey}");

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);

            var adminActions = await _adminActionsRepository.GetAdminActions(new AdminActionsFilterModel
            {
                GameType = gameType,
                Filter = AdminActionsFilterModel.FilterType.ActiveBans,
                Order = AdminActionsFilterModel.OrderBy.CreatedAsc
            });

            await using (var banEntryStream = new MemoryStream())
            {
                await using (var streamWriter = new StreamWriter(banEntryStream))
                {
                    foreach (var adminActionDto in adminActions)
                        streamWriter.WriteLine($"{adminActionDto.Guid} [BANSYNC]-{adminActionDto.Username}");

                    streamWriter.Flush();
                    banEntryStream.Seek(0, SeekOrigin.Begin);

                    await blobClient.UploadAsync(banEntryStream, true);
                }
            }
        }

        public async Task<long> GetBanFileSizeForGame(GameType gameType)
        {
            var blobKey = $"{gameType}-bans.txt";

            _logger.LogInformation($"Retrieving ban file size for {gameType} using blob key {blobKey}");

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);

            if (blobClient.Exists()) return (await blobClient.GetPropertiesAsync()).Value.ContentLength;

            return 0;
        }

        public async Task<Stream> GetBanFileForGame(GameType gameType)
        {
            var blobKey = $"{gameType}-bans.txt";

            _logger.LogInformation($"Retrieving ban file size for {gameType} using blob key {blobKey}");

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);

            if (blobClient.Exists())
            {
                var stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }

            return new MemoryStream();
        }
    }
}