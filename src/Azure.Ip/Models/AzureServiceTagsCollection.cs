using Newtonsoft.Json;

namespace Azure.Ip.Models
{
    public class AzureServiceTagsCollection
    {
        [JsonProperty("changeNumber")]
        public long ChangeNumber { get; set; }

        [JsonProperty("cloud")]
        public string Cloud { get; set; }

        [JsonProperty("values")]
        public AzureServiceTag[] AzureServiceTags { get; set; }
    }
}