using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Azure.Common;
using Azure.Ip.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azure.Ip
{
    public class AzureIpProvider : IAzureIpProvider
    {
        private const string AzureIpInfoListKey = "AzureIpInfoList";
        private const double CacheAbsoluteExpirationInMinutes = 60;
        private readonly ILogger<AzureIpProvider> logger;
        private readonly IMemoryCache memoryCache;
        private readonly IHttpClientFactory httpClientFactory;

        public AzureIpProvider(
            ILogger<AzureIpProvider> logger,
            IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.memoryCache = memoryCache;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IList<AzureIpInfo>> GetAzureIpInfo(string ipOrDomain)
        {
            if (!TryParseIpAddress(ipOrDomain, out string ipAddress))
            {
                logger.LogInformation($"Can not parse {ipOrDomain} to a valid ip address");
                return null;
            }

            var result = new List<AzureIpInfo>();
            var azureIpInfoList = await GetAzureIpInfoListAsync();
            foreach (var ipInfo in azureIpInfoList)
            {
                var ipNetwork = IPNetwork.Parse(ipInfo.IpAddressPrefix);

                if (ipNetwork.Contains(IPAddress.Parse(ipAddress)))
                {
                    ipInfo.IpAddress = ipAddress;
                    logger.LogInformation($"GetAzureIpInfo ipOrDomain = {ipOrDomain}, result = {JsonConvert.SerializeObject(ipInfo)}");
                    result.Add(ipInfo);
                }
            }

            if (result.Count == 0)
            {
                logger.LogInformation($"{ipAddress} is not a known Azure ip address");
            }

            return result;
        }

        public bool TryParseIpAddress(string ipOrDomain, out string result)
        {
            try
            {
                // 1. If input is an ip v4 or v6 address, skip parse
                if (IPAddress.TryParse(ipOrDomain, out var address))
                {
                    switch (address.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                        case AddressFamily.InterNetworkV6:
                            result = ipOrDomain;
                            logger.LogInformation($"{ipOrDomain} is a valid ip address, parse is skipped");
                            return true;
                        default:
                            // Not a valid ip address, do nothing
                            break;
                    }
                }

                // 2. Convert to url and parse
                if (!(ipOrDomain.StartsWith("http://") || ipOrDomain.StartsWith("https://")))
                {
                    ipOrDomain = "http://" + ipOrDomain;
                }

                var tmpUri = new Uri(ipOrDomain);
                ipOrDomain = tmpUri.Host;
                var ipAddresses = Dns.GetHostAddresses(ipOrDomain);
                var ipAddress = ipAddresses[0];
                result = ipAddress.ToString();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error parsing ipOrDomain, error = {ex}");
                result = string.Empty;
                return false;
            }
        }

        private async Task<IList<AzureIpInfo>> GetAzureIpInfoListAsync()
        {
            if (!memoryCache.TryGetValue(AzureIpInfoListKey, out IList<AzureIpInfo> azureIpInfoList))
            {
                azureIpInfoList = await GetAzureIpInfoListFromBlobAsync();
                memoryCache.Set(AzureIpInfoListKey, azureIpInfoList, TimeSpan.FromMinutes(CacheAbsoluteExpirationInMinutes));
                logger.LogInformation($"{azureIpInfoList.Count} rows are added to cache with expiration {CacheAbsoluteExpirationInMinutes} minutes");
            }

            return azureIpInfoList;
        }

        private async Task<IList<AzureIpInfo>> GetAzureIpInfoListFromBlobAsync()
        {
            IList<AzureIpInfo> azureIpInfoList = new List<AzureIpInfo>();
            var clouds = Enum.GetValues(typeof(AzureCloudName));
            foreach (var cloud in clouds)
            {
                string ipFileBlobUrl = $"https://azureiplookup.blob.core.windows.net/ipfiles/{cloud}.json";
                logger.LogInformation($"Getting {cloud} service tags from {ipFileBlobUrl}");
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
