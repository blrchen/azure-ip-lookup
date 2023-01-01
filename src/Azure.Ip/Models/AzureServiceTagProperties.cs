using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Azure.Ip.Models
{
    public class AzureServiceTagProperties
    {
        [JsonProperty("changeNumber")]
        public long ChangeNumber { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("systemService")]
        public string SystemService { get; set; }

        [JsonProperty("addressPrefixes")]
        public ReadOnlyCollection<string> AddressPrefixes { get; set; }

        [JsonProperty("networkFeatures")]
        public ReadOnlyCollection<string> NetworkFeatures { get; set; }
    }
}