using System.Collections.Generic;
using System.Threading.Tasks;
using AzureIpLookup.DataContracts;

namespace AzureIpLookup.Providers
{
    public interface IAzureIpInfoProvider
    {
        Task<AzureIpInfo> GetAzureIpInfo(string ipOrDomain);
    }
}