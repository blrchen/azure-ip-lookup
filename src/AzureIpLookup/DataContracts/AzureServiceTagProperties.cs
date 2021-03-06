﻿using Newtonsoft.Json;

namespace AzureIpLookup.DataContracts
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
        public string[] AddressPrefixes { get; set; }
    }
}