using System.IO;
using System.Net.Http;
using System.Text;
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

        public async Task UploadToBlobAsync(string blobName, string content)
        {
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content ?? ""));
            await blobClient.UploadAsync(ms, true);

            logger.LogInformation($"Successfully uploaded to {blobClient.Uri}");
        }

        public async Task UploadToBlobFromUrlAsync(string blobName, string url)
        {
            var httpClient = new HttpClient();
            await using var responseSteam = await httpClient.GetStreamAsync(url);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(responseSteam, true);
            logger.LogInformation($"Completed upload file {blobName} to Azure Storage {blobClient.Uri}");
        }
    }
}
