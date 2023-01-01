using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Blob;
using Azure.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureIpLookup.Triggers
{
    public class TimerTrigger
    {
        private readonly Dictionary<string, AzureCloudName> downloadIdMapping = new Dictionary<string, AzureCloudName>
        {
            { "56519", AzureCloudName.AzureCloud },
            { "57062", AzureCloudName.AzureChinaCloud },
            { "57063", AzureCloudName.AzureUSGovernment },
            { "57064", AzureCloudName.AzureGermanCloud }
        };
        private readonly Regex fileUriParserRegex = new Regex(@"(https:\/\/download.microsoft.com\/download\/.*?\/ServiceTags_[A-z]+_[0-9]+\.json)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly ILogger<TimerTrigger> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IAzureBlobProvider azureBlobProvider;

        public TimerTrigger(
            ILogger<TimerTrigger> logger,
            IHttpClientFactory httpClientFactory,
            IAzureBlobProvider azureBlobProvider)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.azureBlobProvider = azureBlobProvider;
        }

        // Triggered every day at UTC 1AM
        [FunctionName("SyncServiceTagFilesAsync")]
        public async Task RunAsync([TimerTrigger("0 0 1 * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            await SyncServiceTagFilesAsync();
        }

        public async Task SyncServiceTagFilesAsync()
        {
            foreach (var (downloadId, azureCloudName) in downloadIdMapping)
            {
                string originalDownloadUrl = $"https://www.microsoft.com/en-us/download/confirmation.aspx?id={downloadId}";
                logger.LogInformation($"Fetch service tag file download url for cloud {azureCloudName} from {originalDownloadUrl}");
                using var httpClient = httpClientFactory.CreateClient();
                string responseString = await httpClient.GetStringAsync(new Uri(originalDownloadUrl));
                var matches = fileUriParserRegex.Match(responseString);
                if (matches.Success)
                {
                    string downloadUrl = matches.Value;
                    string blobName = $"{azureCloudName}.json";
                    await azureBlobProvider.UploadToBlobFromUriAsync(blobName, new Uri(downloadUrl));
                    logger.LogInformation($"Completed upload service tag file for cloud {azureCloudName}");
                }
                else
                {
                    logger.LogError($"Failed to parse service tag file download url for cloud {azureCloudName}");
                }
            }

            logger.LogInformation("Completed download service tag files for all clouds");
        }
    }
}
