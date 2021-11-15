using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureIpLookup.Common;
using AzureIpLookup.Providers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureIpLookup.Triggers
{
    public class TimerTrigger
    {
        private readonly ILogger<TimerTrigger> logger;
        private readonly Regex fileUriParserRegex = new Regex(@"(https:\/\/download.microsoft.com\/download\/.*?\/ServiceTags_[A-z]+_[0-9]+\.json)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Dictionary<string, AzureCloudName> downloadIdMapping = new Dictionary<string, AzureCloudName>
        {
            { "56519", AzureCloudName.AzureCloud },
            { "57062", AzureCloudName.AzureChinaCloud },
            { "57063", AzureCloudName.AzureUSGovernment },
            { "57064", AzureCloudName.AzureGermanCloud }
        };
        private readonly IAzureStorageProvider azureStorageProvider;

        public TimerTrigger(ILogger<TimerTrigger> logger, IAzureStorageProvider azureStorageProvider)
        {
            this.logger = logger;
            this.azureStorageProvider = azureStorageProvider;
        }

        // Triggered every day at UTC 1AM
        [FunctionName("DownloadAzureIpRangeFiles")]
        public async Task DownloadAzureIpRangeFiles([TimerTrigger("0 0 1 * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            await DownloadAzureIpRangeFilesAsync();
        }

        // Triggered every day at UTC 1AM
        [FunctionName("IngestServiceTagsCsv")]
        public async Task IngestServiceTagsCsv([TimerTrigger("1 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            var list = await azureStorageProvider.GetAzureIpInfoListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("ServiceTagId,IpAddress,IpAddressPrefix,Region,SystemService,NetworkFeatures");
            foreach (var i in list)
            {
                sb.AppendLine($"{i.ServiceTagId},{i.IpAddress}, {i.IpAddressPrefix}, {i.Region}, {i.SystemService}, {i.NetworkFeatures}");
            }

            await azureStorageProvider.UploadBlobAsync("ServiceTags.csv", sb.ToString());
        }

        public async Task DownloadAzureIpRangeFilesAsync()
        {
            foreach (var (downloadId, azureCloudName) in downloadIdMapping)
            {
                string targetUri = $"https://www.microsoft.com/en-us/download/confirmation.aspx?id={downloadId}";
                logger.LogInformation($"Download ip range file for cloud {azureCloudName} from url {targetUri}");
                var httpClient = new HttpClient();
                string responseString = await httpClient.GetStringAsync(targetUri);
                var matches = fileUriParserRegex.Match(responseString);
                if (matches.Success)
                {
                    string downloadUrl = matches.Value;
                    string blobName = $"{azureCloudName}.json";
                    await azureStorageProvider.UploadBlobFromUrlAsync(blobName, downloadUrl);
                    logger.LogInformation($"Completed upload ip range file for cloud {azureCloudName}");
                }
                else
                {
                    logger.LogError($"Failed to parse ip range file download url for cloud {azureCloudName}");
                }
            }

            logger.LogInformation("Completed download ip range files for all clouds");
        }
    }
}
