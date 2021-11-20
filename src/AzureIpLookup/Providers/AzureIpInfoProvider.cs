using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AzureIpLookup.DataContracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureIpLookup.Providers
{
    public class AzureIpInfoProvider : IAzureIpInfoProvider
    {
        private const string AzureIpInfoListKey = "AzureIpInfoList";
        private const double CacheAbsoluteExpirationInMinutes = 60;
        private readonly IAzureStorageProvider azureStorageProvider;
        private readonly IIpAddressProvider ipAddressProvider;
        private readonly ILogger<AzureIpInfoProvider> logger;
        private readonly IMemoryCache memoryCache;

        public AzureIpInfoProvider(
            IAzureStorageProvider azureStorageProvider,
            IIpAddressProvider ipAddressProvider,
            ILogger<AzureIpInfoProvider> logger,
            IMemoryCache memoryCache)
        {
            this.azureStorageProvider = azureStorageProvider;
            this.ipAddressProvider = ipAddressProvider;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        public async Task<IList<AzureIpInfo>> GetAzureIpInfo(string ipOrDomain)
        {
            if (!ipAddressProvider.TryParseIpAddress(ipOrDomain, out string ipAddress))
            {
                logger.LogInformation($"Can not parse {ipOrDomain} to a valid ip address");
                return null;
            }

            var result = new List<AzureIpInfo>();
            var azureIpInfoList = await GetAzureIpInfoList();
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

        private async Task<IList<AzureIpInfo>> GetAzureIpInfoList()
        {
            if (!memoryCache.TryGetValue(AzureIpInfoListKey, out IList<AzureIpInfo> azureIpInfoList))
            {
                azureIpInfoList = await azureStorageProvider.GetAzureIpInfoListAsync();
                memoryCache.Set(AzureIpInfoListKey, azureIpInfoList, TimeSpan.FromMinutes(CacheAbsoluteExpirationInMinutes));
                logger.LogInformation($"{azureIpInfoList.Count} rows are added to cache with expiration {CacheAbsoluteExpirationInMinutes} minutes");
            }

            return azureIpInfoList;
        }
    }
}
