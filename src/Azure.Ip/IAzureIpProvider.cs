using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Ip.Models;

namespace Azure.Ip
{
    public interface IAzureIpProvider
    {
        Task<IList<AzureIpInfo>> GetAzureIpInfo(string ipOrDomain);

        // TODO: TryParseIpAddress is only consumed by ut, can be converted to internal
        public bool TryParseIpAddress(string ipOrDomain, out string result);
    }
}