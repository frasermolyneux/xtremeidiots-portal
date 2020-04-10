using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using XI.CommonTypes;
using XI.Portal.Maps.Configuration;
using XI.Portal.Maps.Extensions;
using XI.Portal.Maps.Properties;

namespace XI.Portal.Maps.Repository
{
    public class MapImageRepository : IMapImageRepository
    {
        private readonly IMapImageRepositoryOptions _options;

        public MapImageRepository(IMapImageRepositoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        private Dictionary<string, Uri> UriCache { get; } = new Dictionary<string, Uri>();

        public async Task<Uri> GetMapImage(GameType gameType, string mapName)
        {
            var blobKey = $"{gameType}_{mapName}.jpg";
            if (UriCache.ContainsKey(blobKey)) return UriCache[blobKey];

            var blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_options.StorageContainerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobKey);
            if (blobClient.Exists())
            {
                UriCache.Add(blobKey, blobClient.Uri);
                return blobClient.Uri;
            }

            try
            {
                var gameTrackerImageUrl = $"https://image.gametracker.com/images/maps/160x120/{gameType.ToGameTrackerShortName()}/{mapName}.jpg";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                    var data = client.DownloadData(new Uri(gameTrackerImageUrl));

                    await blobClient.UploadAsync(new MemoryStream(data));

                    UriCache.Add(blobKey, blobClient.Uri);
                    return blobClient.Uri;
                }
            }
            catch
            {
                // swallow
            }

            const string defaultImageBlob = "default-image.jpg";
            if (UriCache.ContainsKey(defaultImageBlob)) return UriCache[defaultImageBlob];

            var defaultImageBlobClient = containerClient.GetBlobClient(defaultImageBlob);

            var stream = new MemoryStream(Resources.noimage);
            if (!defaultImageBlobClient.Exists()) defaultImageBlobClient.Upload(stream);

            UriCache.Add(defaultImageBlob, defaultImageBlobClient.Uri);
            return defaultImageBlobClient.Uri;
        }
    }
}