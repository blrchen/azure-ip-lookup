using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureIpLookup.Common;
using AzureIpLookup.DataContracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureIpLookup.Providers
{
    public class AzureStorageProvider : IAzureStorageProvider
    {
        private readonly BlobContainerClient blobContainerClient;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<AzureStorageProvider> logger;

        public AzureStorageProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<AzureStorageProvider> logger)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            this.blobContainerClient = new BlobContainerClient(storageConnectionString, Constants.StorageContainerName);
            this.blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task UploadBlobAsync(string blobName, string content)
        {
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content ?? ""));
            await blobClient.UploadAsync(ms, true);

            logger.LogInformation($"Completed upload file {blobName} to Azure Storage {blobClient.Uri}");
        }

        public async Task UploadBlobFromUrlAsync(string blobName, string url)
        {
            var httpClient = new HttpClient();
            await using var responseSteam = await httpClient.GetStreamAsync(url);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(responseSteam, true);
            logger.LogInformation($"Completed upload file {blobName} to Azure Storage {blobClient.Uri}");
        }

        public async Task<IList<AzureIpInfo>> GetAzureIpInfoListAsync()
        {
            IList<AzureIpInfo> azureIpInfoList = new List<AzureIpInfo>();
            var clouds = Enum.GetValues(typeof(AzureCloudName));
            foreach (var cloud in clouds)
            {
                string ipFileBlobUrl = $"https://azureiplookup.blob.core.windows.net/ipfiles/{cloud}.json";
                logger.LogInformation($"Getting Azure ip info for {cloud} from {ipFileBlobUrl}");
                string jsonResponseMessage = await httpClientFactory.CreateClient().GetStringAsync(ipFileBlobUrl);
                var azureServiceTagsCollection = JsonConvert.DeserializeObject<AzureServiceTagsCollection>(jsonResponseMessage);

                foreach (var azureServiceTag in azureServiceTagsCollection.AzureServiceTags)
                {
                    foreach (string addressPrefix in azureServiceTag.Properties.AddressPrefixes)
                    {
                        azureIpInfoList.Add(new AzureIpInfo
                        {
                            ServiceTagId = azureServiceTag.Id,
                            Region = azureServiceTag.Properties.Region,

                            // Platform = azureServiceTag.Properties.Platform, // Platform is always Azure
                            SystemService = azureServiceTag.Properties.SystemService,
                            IpAddressPrefix = addressPrefix,
                            NetworkFeatures = azureServiceTag.Properties.NetworkFeatures == null ? "" : string.Join(' ', azureServiceTag.Properties.NetworkFeatures)
                        });
                    }
                }
            }

            return azureIpInfoList;
        }
    }
}
