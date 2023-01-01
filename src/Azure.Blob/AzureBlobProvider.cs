using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Blob
{
    public class AzureBlobProvider : IAzureBlobProvider
    {
        private readonly ILogger<AzureBlobProvider> logger;
        private readonly BlobContainerClient blobContainerClient;

        public AzureBlobProvider(
            ILogger<AzureBlobProvider> logger,
            BlobContainerClient blobContainerClient)
        {
            this.logger = logger;
            this.blobContainerClient = blobContainerClient;
            this.blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task UploadToBlobFromUriAsync(string blobName, Uri uri)
        {
            using var httpClient = new HttpClient();
            await using var responseSteam = await httpClient.GetStreamAsync(uri);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(responseSteam, true);
            logger.LogInformation($"Completed upload file {blobName} to Azure Storage {blobClient.Uri}");
        }
    }
}
