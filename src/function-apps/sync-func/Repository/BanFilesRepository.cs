using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.SyncFunc.Interfaces;

namespace XtremeIdiots.Portal.SyncFunc.Repository
{
    public class BanFilesRepository : IBanFilesRepository
    {
        private readonly ILogger<BanFilesRepository> _logger;
        private readonly IBanFilesRepositoryOptions _options;
        private readonly IRepositoryApiClient repositoryApiClient;

        public BanFilesRepository(
            ILogger<BanFilesRepository> logger,
            IBanFilesRepositoryOptions options,
            IRepositoryApiClient repositoryApiClient
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            this.repositoryApiClient = repositoryApiClient;
        }

        public async Task RegenerateBanFileForGame(string gameType)
        {
            var blobKey = $"{gameType}-bans.txt";

            _logger.LogInformation($"Regenerating ban file for {gameType} using blob key {blobKey}");

            var blobServiceClient = new BlobServiceClient(_options.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);

            var adminActions = await repositoryApiClient.AdminActions.GetAdminActions(gameType, null, null, "ActiveBans", 0, 0, "CreatedAsc");

            var externalBansStream = await GetExternalBanFileForGame(gameType);
            externalBansStream.Seek(externalBansStream.Length, SeekOrigin.Begin);

            await using (var streamWriter = new StreamWriter(externalBansStream))
            {
                foreach (var adminActionDto in adminActions)
                    streamWriter.WriteLine($"{adminActionDto.Guid} [BANSYNC]-{adminActionDto.Username}");

                streamWriter.Flush();
                externalBansStream.Seek(0, SeekOrigin.Begin);
                await blobClient.UploadAsync(externalBansStream, true);
            }
        }

        public async Task<long> GetBanFileSizeForGame(string gameType)
        {
            var blobKey = $"{gameType}-bans.txt";

            _logger.LogInformation($"Retrieving ban file size for {gameType} using blob key {blobKey}");

            var blobServiceClient = new BlobServiceClient(_options.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);

            if (blobClient.Exists()) return (await blobClient.GetPropertiesAsync()).Value.ContentLength;

            return 0;
        }

        public async Task<Stream> GetBanFileForGame(string gameType)
        {
            var blobKey = $"{gameType}-bans.txt";

            _logger.LogInformation($"Retrieving ban file for {gameType} using blob key {blobKey}");

            return await GetFileStream(blobKey);
        }

        private async Task<Stream> GetExternalBanFileForGame(string gameType)
        {
            var blobKey = $"{gameType}-external.txt";

            _logger.LogInformation($"Retrieving ban file size for {gameType} using blob key {blobKey}");

            return await GetFileStream(blobKey);
        }

        private async Task<Stream> GetFileStream(string blobKey)
        {
            var blobServiceClient = new BlobServiceClient(_options.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);

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