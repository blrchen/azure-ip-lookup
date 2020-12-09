using Newtonsoft.Json;

namespace AzureIpLookup.DataContracts
{
    public class AzureServiceTag
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("properties")]
        public AzureServiceTagProperties Properties { get; set; }
    }
}