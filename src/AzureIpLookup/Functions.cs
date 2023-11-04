using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: FunctionsStartup(typeof(AzureIpLookup.Startup))]
namespace AzureIpLookup
{
    // This class is used for dependency injection setup when the functions runtime starts up.
    // Details see https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
    public class Startup : FunctionsStartup
    {
        private const string BlobContainerName = "ipfiles";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddLogging();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton(provide => new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), BlobContainerName));
            builder.Services.AddSingleton<IAzureIpAddressService, AzureIpAddressService>();
            builder.Services.AddSingleton<IAzureStorageClient, AzureStorageClient>();
        }
    }

    public class HttpTrigger
    {
        private readonly IAzureIpAddressService azureIpAddressService;
        private readonly ILogger<HttpTrigger> logger;

        public HttpTrigger(
            IAzureIpAddressService azureIpIpAddressService,
            ILogger<HttpTrigger> logger)
        {
            this.azureIpAddressService = azureIpIpAddressService;
            this.logger = logger;
        }

        [FunctionName("GetAzureIpAddress")]
        public async Task<IActionResult> GetAzureIpAddress([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ipAddress")] HttpRequest request)
        {
            string ipOrDomain = request.Query["ipOrDomain"];
            if (string.IsNullOrEmpty(ipOrDomain))
            {
                return new BadRequestObjectResult("ipOrDomain can not be null");
            }

            var result = await azureIpAddressService.GetAzureIpAddressList(ipOrDomain);

            logger.LogInformation("Function GetAzureIpAddress completed successfully");

            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }
    }

    public class AzureIpAddressService : IAzureIpAddressService
    {
        private const string AzureIpAddressListKey = "AzureIpAddressList";
        private const double CacheAbsoluteExpirationInMinutes = 60;
        private readonly ILogger<AzureIpAddressService> logger;
        private readonly IMemoryCache memoryCache;

        public AzureIpAddressService(
            ILogger<AzureIpAddressService> logger,
            IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        public async Task<IList<AzureIpAddress>> GetAzureIpAddressList(string ipOrDomain)
        {
            if (!TryParseIpAddress(ipOrDomain, out string ipAddress))
            {
                logger.LogInformation($"Can not parse {ipOrDomain} to a valid ip address");
                return null;
            }

            var result = new List<AzureIpAddress>();
            var azureIpAddressList = await GetAzureIpAddressListFromCacheOrBlob();
            foreach (var azureIpAddress in azureIpAddressList)
            {
                var ipNetwork = IPNetwork.Parse(azureIpAddress.IpAddressPrefix);

                if (!ipNetwork.Contains(IPAddress.Parse(ipAddress)))
                {
                    continue;
                }

                azureIpAddress.IpAddress = ipAddress;
                logger.LogInformation($"GetAzureIpAddress ipOrDomain = {ipOrDomain}, result = {JsonConvert.SerializeObject(azureIpAddress)}");
                result.Add(azureIpAddress);
            }

            if (result.Count == 0)
            {
                logger.LogInformation($"{ipAddress} is not a known Azure ip address");
            }

            return result;
        }

        public bool TryParseIpAddress(string ipOrDomain, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrEmpty(ipOrDomain))
            {
                return false;
            }

            try
            {
                if (IPAddress.TryParse(ipOrDomain, out var address))
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork || address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        result = ipOrDomain;
                        logger.LogInformation($"{ipOrDomain} is a valid ip address, parse is skipped");
                        return true;
                    }
                }

                var ipAddresses = Dns.GetHostAddresses(ipOrDomain);
                var ipAddress = ipAddresses[0];
                result = ipAddress.ToString();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error parsing ipOrDomain, error = {ex}");
                return false;
            }
        }

        private async Task<IList<AzureIpAddress>> GetAzureIpAddressListFromCacheOrBlob()
        {
            if (memoryCache.TryGetValue(AzureIpAddressListKey, out IList<AzureIpAddress> azureIpAddressList))
            {
                return azureIpAddressList;
            }

            azureIpAddressList = await GetAzureIpAddressListFromBlob();
            memoryCache.Set(AzureIpAddressListKey, azureIpAddressList, TimeSpan.FromMinutes(CacheAbsoluteExpirationInMinutes));
            logger.LogInformation($"{azureIpAddressList.Count} rows are added to cache with expiration {CacheAbsoluteExpirationInMinutes} minutes");

            return azureIpAddressList;
        }

        private async Task<IList<AzureIpAddress>> GetAzureIpAddressListFromBlob()
        {
            IList<AzureIpAddress> azureIpAddressList = new List<AzureIpAddress>();
            var clouds = Enum.GetValues(typeof(AzureCloudName));
            foreach (var cloud in clouds)
            {
                string ipFileBlobUrl = $"https://azureiplookup.blob.core.windows.net/ipfiles/{cloud}.json";
                logger.LogInformation($"Getting {cloud} service tags from {ipFileBlobUrl}");

                using var httpClient = new HttpClient();
                string jsonResponseMessage = await httpClient.GetStringAsync(new Uri(ipFileBlobUrl));
                var azureServiceTagsCollection = JsonConvert.DeserializeObject<AzureServiceTagsRoot>(jsonResponseMessage);

                foreach (var azureServiceTag in azureServiceTagsCollection.AzureServiceTags)
                {
                    foreach (string addressPrefix in azureServiceTag.Properties.AddressPrefixes)
                    {
                        azureIpAddressList.Add(new AzureIpAddress
                        {
                            ServiceTagId = azureServiceTag.Id,
                            Region = azureServiceTag.Properties.Region,
                            RegionId = azureServiceTag.Properties.RegionId,
                            SystemService = azureServiceTag.Properties.SystemService,
                            IpAddressPrefix = addressPrefix,
                            NetworkFeatures = azureServiceTag.Properties.NetworkFeatures == null ? "" : string.Join(' ', azureServiceTag.Properties.NetworkFeatures)
                        });
                    }
                }
            }

            return azureIpAddressList;
        }
    }

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
        private readonly IAzureStorageClient azureStorageClient;

        public TimerTrigger(
            ILogger<TimerTrigger> logger,
            IAzureStorageClient azureBlobProvider)
        {
            this.logger = logger;
            this.azureStorageClient = azureBlobProvider;
        }

        [FunctionName("SyncServiceTagFiles")]
        public async Task RunAsync([TimerTrigger("0 0 1 * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            await SyncServiceTagFiles();
        }

        public async Task SyncServiceTagFiles()
        {
            foreach (var (downloadId, azureCloudName) in downloadIdMapping)
            {
                string originalDownloadUrl = $"https://www.microsoft.com/en-us/download/confirmation.aspx?id={downloadId}";
                logger.LogInformation($"Fetch service tag file download url for cloud {azureCloudName} from {originalDownloadUrl}");
                using var httpClient = new HttpClient();
                string responseString = await httpClient.GetStringAsync(new Uri(originalDownloadUrl));
                var matches = fileUriParserRegex.Match(responseString);
                if (matches.Success)
                {
                    string downloadUrl = matches.Value;
                    string blobName = $"{azureCloudName}.json";
                    await azureStorageClient.UploadToBlobFromUri(blobName, new Uri(downloadUrl));
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

    public class AzureStorageClient : IAzureStorageClient
    {
        private readonly ILogger<AzureStorageClient> logger;
        private readonly BlobContainerClient blobContainerClient;

        public AzureStorageClient(
            ILogger<AzureStorageClient> logger,
            BlobContainerClient blobContainerClient)
        {
            this.logger = logger;
            this.blobContainerClient = blobContainerClient;
            this.blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task UploadToBlobFromUri(string blobName, Uri uri)
        {
            using var httpClient = new HttpClient();
            await using var responseSteam = await httpClient.GetStreamAsync(uri);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(responseSteam, true);
            logger.LogInformation($"Completed upload file {blobName} to Azure Storage {blobClient.Uri}");
        }
    }
}