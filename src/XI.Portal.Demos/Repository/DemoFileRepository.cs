using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using XI.Portal.Demos.Interfaces;

namespace XI.Portal.Demos.Repository
{
    public class DemoFileRepository : IDemoFileRepository
    {
        private readonly IDemosRepositoryOptions _options;

        public DemoFileRepository(
            IDemosRepositoryOptions options
            )
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }


        public async Task CreateDemo(string fileName, string filePath)
        {
            var blobKey = fileName;

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);

            var data = File.ReadAllBytes(filePath);
            await blobClient.UploadAsync(new MemoryStream(data));
        }

        public async Task<Uri> GetDemoUrl(string fileName)
        {
            var blobKey = fileName;

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            var blobClient = containerClient.GetBlobClient(blobKey);

            return blobClient.Uri;
        }
    }
}