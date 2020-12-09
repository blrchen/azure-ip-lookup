using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureIpLookup.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureIpLookup.Triggers
{
    public class TimerTrigger
    {
        private readonly ILogger<TimerTrigger> logger;
        private readonly Regex fileUriParserRegex = new Regex(@"(https:\/\/download.microsoft.com\/download\/.*?\/ServiceTags_[A-z]+_[0-9]+\.json)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Dictionary<string, AzureCloudName> downloadIdMapping = new Dictionary<string, AzureCloudName>()
        {
            { "56519", AzureCloudName.AzureCloud },
            { "57062", AzureCloudName.AzureChinaCloud },
            { "57063", AzureCloudName.AzureUSGovernment },
            { "57064", AzureCloudName.AzureGermanCloud }
        };

        public TimerTrigger(ILogger<TimerTrigger> logger)
        {
            this.logger = logger;
        }

        // Triggered every day at midnight - 1am
        [FunctionName("DownloadAzureIpRangeFiles")]
        public async Task DownloadAzureIpRangeFiles([TimerTrigger("0 0 1 * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            await DownloadAzureIpRangeFiles();
        }

        private async Task DownloadAzureIpRangeFiles()
        {
            foreach (var downloadId in downloadIdMapping)
            {
                logger.LogInformation($"Fetching {downloadId.Value} ip range file download url");
                string targetUri = $"https://www.microsoft.com/en-us/download/confirmation.aspx?id={downloadId.Key}";
                var httpClient = new HttpClient();
                string responseString = await httpClient.GetStringAsync(targetUri);
                var matches = fileUriParserRegex.Match(responseString);
                if (matches.Success)
                {
                    string downloadUrl = matches.Value;
                    logger.LogInformation($"Downloading {downloadId.Value} ip range files from from {downloadUrl}");
                    using (var responseSteam = await httpClient.GetStreamAsync(downloadUrl))
                    {
                        string blobName = $"{downloadId.Value}.json";
                        await UploadToBlob(blobName, responseSteam, logger);
                    }
                }
            }

            logger.LogInformation("Completed download ip range files for all clouds");
        }

        private async Task UploadToBlob(string blobName, Stream blobStream, ILogger logger)
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new Exception("Storage connection string can not be null");
            }

            var container = new BlobContainerClient(storageConnectionString, Constants.StorageContainerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
            var blob = container.GetBlobClient(blobName);
            await blob.UploadAsync(blobStream, true);
            logger.LogInformation($"Successfully uploaded to {blob.Uri}");
        }
    }
}
