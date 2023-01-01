using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Azure.Ip.Models
{
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