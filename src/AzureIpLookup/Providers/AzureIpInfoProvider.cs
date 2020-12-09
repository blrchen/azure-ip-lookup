using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AzureIpLookup.Common;
using AzureIpLookup.DataContracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureIpLookup.Providers
{
    public class AzureIpInfoProvider : IAzureIpInfoProvider
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<AzureIpInfoProvider> logger;

        public AzureIpInfoProvider(IHttpClientFactory httpClientFactory, ILogger<AzureIpInfoProvider> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
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
                    return ipInfo;
                }
            }

            logger.LogInformation($"{ipAddress} is not a known Azure ip address");

            return new AzureIpInfo();
        }

        private async Task<List<AzureIpInfo>> GetAzureIpInfoList()
        {
            List<AzureIpInfo> azureIpInfoList = new List<AzureIpInfo>();
            var clouds = Enum.GetValues(typeof(AzureCloudName));
            foreach (var cloud in clouds)
            {
                string ipFileBlobUrl = $"https://azureiplookup.blob.core.windows.net/ipfiles/{cloud}.json";
                logger.LogInformation($"Getting Azure ip info for {cloud} from {ipFileBlobUrl}");
                string jsonResponseMessage = await this.httpClientFactory.CreateClient().GetStringAsync(ipFileBlobUrl);
                var azureServiceTagsCollection = JsonConvert.DeserializeObject<AzureServiceTagsCollection>(jsonResponseMessage);

                foreach (var azureServiceTag in azureServiceTagsCollection.AzureServiceTags)
                {
                    if (string.IsNullOrWhiteSpace(azureServiceTag.Properties.Region))
                    {
                        continue;
                    }

                    foreach (string addressPrefix in azureServiceTag.Properties.AddressPrefixes)
                    {
                        azureIpInfoList.Add(new AzureIpInfo
                        {
                            ServiceTagId = azureServiceTag.Id,
                            Region = azureServiceTag.Properties.Region,

                            // Platform = azureServiceTag.Properties.Platform, // Platform is always Azure
                            SystemService = azureServiceTag.Properties.SystemService,
                            IpAddressPrefix = addressPrefix
                        });
                    }
                }
            }

            return azureIpInfoList;
        }
    }
}
