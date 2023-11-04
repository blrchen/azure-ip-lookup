using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace AzureIpLookup
{
    public class AzureIpAddress
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
        [JsonProperty("regionId")]
        public string RegionId { get; set; }

        // public string Platform { get; set; }
        [JsonProperty("systemService")]
        public string SystemService { get; set; }

        [JsonProperty("networkFeatures")]
        public string NetworkFeatures { get; set; }
    }
    public class AzureServiceTag
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("properties")]
        public AzureServiceTagProperties Properties { get; set; }
    }
    public class AzureServiceTagProperties
    {
        [JsonProperty("changeNumber")]
        public long ChangeNumber { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("regionId")]
        public string RegionId { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("systemService")]
        public string SystemService { get; set; }

        [JsonProperty("addressPrefixes")]
        public ReadOnlyCollection<string> AddressPrefixes { get; set; }

        [JsonProperty("networkFeatures")]
        public ReadOnlyCollection<string> NetworkFeatures { get; set; }
    }
    public class AzureServiceTagsRoot
    {
        [JsonProperty("changeNumber")]
        public long ChangeNumber { get; set; }

        [JsonProperty("cloud")]
        public string Cloud { get; set; }

        [JsonProperty("values")]
        public ReadOnlyCollection<AzureServiceTag> AzureServiceTags { get; set; }
    }
}
