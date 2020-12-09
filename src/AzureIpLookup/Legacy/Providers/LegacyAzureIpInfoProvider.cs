using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using AzureIpLookup.Common;
using AzureIpLookup.Legacy.Contracts;

namespace AzureIpLookup.Legacy.Providers
{
    public class LegacyAzureIpInfoProvider : ILegacyAzureIpInfoProvider
    {
        public LegacyAzureIpInfoProvider()
        {
        }

        public async Task<LegacyAzureIpInfo> GetLegacyAzureIpInfo(string ipAddressOrUrl)
        {
            var regionNames = GetRegionNames();
            var subnets = await GetSubnetDictionary();

            string ipAddress = Utils.ConvertToIpAddress(ipAddressOrUrl);
            var result = new LegacyAzureIpInfo() { IpAddress = ipAddress };
            foreach (var net in subnets.Keys)
            {
                if (net.Contains(IPAddress.Parse(ipAddress)))
                {
                    var regionAlias = subnets[net];
                    result.Cloud = regionNames[regionAlias].Cloud;
                    result.RegionId = regionNames[regionAlias].RegionId;
                    result.Region = regionNames[regionAlias].Region;
                    result.Location = regionNames[regionAlias].Location;
                    break;
                }
            }

            return result;
        }

        private IDictionary<string, LegacyCloudRegion> GetRegionNames()
        {
            return new Dictionary<string, LegacyCloudRegion>
            {
                // Azure, sorted by PublicIPs.xml
                // TODO: Get data from common.js
                { "uswest", new LegacyCloudRegion { Cloud = "Azure", RegionId = "uswest", Region = "West US", Location = "California" } },
                { "useast", new LegacyCloudRegion { Cloud = "Azure", RegionId = "useast", Region = "East US", Location = "Iowa" } },
                { "useast2", new LegacyCloudRegion { Cloud = "Azure", RegionId = "useast2", Region = "East US 2", Location = "Iowa" } },
                { "usnorth", new LegacyCloudRegion { Cloud = "Azure", RegionId = "usnorth", Region = "North Central US", Location = "Illinois" } },
                { "uswest2", new LegacyCloudRegion { Cloud = "Azure", RegionId = "uswest2", Region = "West US 2", Location = "West US 2" } },
                { "ussouth", new LegacyCloudRegion { Cloud = "Azure", RegionId = "ussouth", Region = "SouthCentral US", Location = "Texas" } },
                { "uscentral", new LegacyCloudRegion { Cloud = "Azure", RegionId = "uscentral", Region = "Central US", Location = "Iowa" } },
                { "europewest", new LegacyCloudRegion { Cloud = "Azure", RegionId = "europewest", Region = "West Europe", Location = "Netherlands" } },
                { "europenorth", new LegacyCloudRegion { Cloud = "Azure", RegionId = "europenorth", Region = "North Europe", Location = "Ireland" } },
                { "asiaeast", new LegacyCloudRegion { Cloud = "Azure", RegionId = "asiaeast", Region = "East Asia", Location = "Hong Kong" } },
                { "asiasoutheast", new LegacyCloudRegion { Cloud = "Azure", RegionId = "asiasoutheast", Region = "Southeast Asia", Location = "Singapore" } },
                { "japaneast", new LegacyCloudRegion { Cloud = "Azure", RegionId = "japaneast", Region = "Japan East", Location = "Saitama Prefecture" } },
                { "japanwest", new LegacyCloudRegion { Cloud = "Azure", RegionId = "japanwest", Region = "Japan West", Location = "Osaka Prefecture" } },
                { "brazilsouth", new LegacyCloudRegion { Cloud = "Azure", RegionId = "brazilsouth", Region = "Brazil South", Location = "Sao Paulo State" } },
                { "australiaeast", new LegacyCloudRegion { Cloud = "Azure", RegionId = "australiaeast", Region = "Australia East", Location = "New South Wales" } },
                { "australiasoutheast", new LegacyCloudRegion { Cloud = "Azure", RegionId = "australiasoutheast", Region = "Australia Southeast", Location = "Victoria" } },
                { "indiasouth", new LegacyCloudRegion { Cloud = "Azure", RegionId = "indiasouth", Region = "South India", Location = "Chennai" } },
                { "indiawest", new LegacyCloudRegion { Cloud = "Azure", RegionId = "indiawest", Region = "West India", Location = "Mumbai" } },
                { "indiacentral", new LegacyCloudRegion { Cloud = "Azure", RegionId = "indiacentral", Region = "Central India", Location = "Pune" } },
                { "canadacentral", new LegacyCloudRegion { Cloud = "Azure", RegionId = "canadacentral", Region = "Canada Central", Location = "Toronto" } },
                { "canadaeast", new LegacyCloudRegion { Cloud = "Azure", RegionId = "canadaeast", Region = "Canada East", Location = "Quebec City" } },
                { "uswestcentral", new LegacyCloudRegion { Cloud = "Azure", RegionId = "uswestcentral", Region = "West Central US", Location = "West Central US" } },
                { "ukwest", new LegacyCloudRegion { Cloud = "Azure", RegionId = "ukwest", Region = "UK West", Location = "Cardiff" } },
                { "uksouth", new LegacyCloudRegion { Cloud = "Azure", RegionId = "uksouth", Region = "UK South", Location = "London" } },
                { "koreasouth", new LegacyCloudRegion { Cloud = "Azure", RegionId = "koreasouth", Region = "Korea South", Location = "Busan" } },
                { "koreacentral", new LegacyCloudRegion { Cloud = "Azure", RegionId = "koreacentral", Region = "Korea Central", Location = "Seoul" } },
                { "chinaeast", new LegacyCloudRegion { Cloud = "Azure", RegionId = "chinaeast", Region = "China East", Location = "Shanghai" } },
                { "chinanorth", new LegacyCloudRegion { Cloud = "Azure", RegionId = "chinanorth", Region = "China North", Location = "Beijing" } },
            };
        }

        private async Task<IDictionary<IPNetwork, string>> GetSubnetDictionary()
        {
            const string AzureIpRangeFileList = "PublicIPs.xml;PublicIPs_MC.xml";
            var subnets = new Dictionary<IPNetwork, string>();
            foreach (string fileName in AzureIpRangeFileList.Split(';'))
            {
                string blobUrl = $"https://azureiplookup.blob.core.windows.net/legacy-ipfiles/{fileName}";
                var httpClient = new HttpClient();
                string xmlContent = await httpClient.GetStringAsync(blobUrl);
                var xmlDoc = new XmlDocument();

                xmlDoc.LoadXml(xmlContent);
                var root = xmlDoc.DocumentElement;
                foreach (XmlElement ele in root)
                {
                    string region = ele.GetAttribute("Name");
                    foreach (XmlElement ipRange in ele)
                    {
                        var subnet = ipRange.GetAttribute("Subnet");
                        if (IPNetwork.TryParse(subnet, out IPNetwork net))
                        {
                            if (!subnets.ContainsKey(net))
                            {
                                subnets.Add(net, region);
                            }
                        }
                    }
                }
            }

            return subnets;
        }
    }
}
