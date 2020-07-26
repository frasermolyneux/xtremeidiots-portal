using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using XI.CommonTypes;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Repository
{
    public class ExternalBansRepository : IExternalBansRepository
    {
        private readonly IExternalBansRepositoryOptions _options;

        public ExternalBansRepository(IExternalBansRepositoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task GetBanFileForGame(GameType gameType)
        {
            var blobKey = $"{gameType}-external-bans.txt";

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);
            if (blobClient.Exists())
            {
               


                //UriCache.Add(blobKey, blobClient.Uri);
                //return blobClient.Uri;
            }
        }
    }
}