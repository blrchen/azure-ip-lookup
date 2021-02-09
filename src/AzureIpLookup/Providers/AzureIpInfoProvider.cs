using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AzureIpLookup.Common;
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
        private readonly ILogger<AzureIpInfoProvider> logger;
        private readonly IMemoryCache memoryCache;

        public AzureIpInfoProvider(
            IAzureStorageProvider azureStorageProvider,
            ILogger<AzureIpInfoProvider> logger,
            IMemoryCache memoryCache)
        {
            this.azureStorageProvider = azureStorageProvider;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        public async Task<AzureIpInfo> GetAzureIpInfo(string ipOrDomain)
        {
            string ipAddress = Utils.ConvertToIpAddress(ipOrDomain);
            var azureIpInfoList = await GetAzureIpInfoList();

            foreach (var ipInfo in azureIpInfoList)
            {
                var ipNetwork = IPNetwork.Parse(ipInfo.IpAddressPrefix);

                if (ipNetwork.Contains(IPAddress.Parse(ipAddress)))
                {
                    ipInfo.IpAddress = ipAddress;
                    logger.LogInformation($"GetAzureIpInfo ipOrDomain = {ipOrDomain}, result = {JsonConvert.SerializeObject(ipInfo)}");
                    return ipInfo;
                }
            }

            logger.LogInformation($"{ipAddress} is not a known Azure ip address");

            return new AzureIpInfo();
        }

        private async Task<IList<AzureIpInfo>> GetAzureIpInfoList()
        {
            if (!memoryCache.TryGetValue(AzureIpInfoListKey, out IList<AzureIpInfo> azureIpInfoList))
            {
                azureIpInfoList = await azureStorageProvider.GetAzureIpInfoListAsync();
                memoryCache.Set(AzureIpInfoListKey, azureIpInfoList, TimeSpan.FromMinutes(CacheAbsoluteExpirationInMinutes));
                logger.LogInformation($"Added {azureIpInfoList.Count} rows to cache");
            }

            return azureIpInfoList;
        }
    }
}
