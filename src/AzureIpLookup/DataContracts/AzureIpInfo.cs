using Newtonsoft.Json;

namespace AzureIpLookup.DataContracts
{
    public class AzureIpInfo
    {
        [JsonProperty("serviceTagId")]
        public string ServiceTagId { get; set; }

        // public string ServiceTagName { get; set; }
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        [JsonProperty("ipAddressPrefix")]
        public string IpAddressPrefix { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }

        // public string Platform { get; set; }
        [JsonProperty("systemService")]
        public string SystemService { get; set; }

        [JsonProperty("networkFeatures")]
        public string NetworkFeatures { get; set; }
    }
}
