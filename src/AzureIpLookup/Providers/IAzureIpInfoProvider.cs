using System.Collections.Generic;
using System.Threading.Tasks;
using AzureIpLookup.DataContracts;

namespace AzureIpLookup.Providers
{
    public interface IAzureIpInfoProvider
    {
        Task<IList<AzureIpInfo>> GetAzureIpInfo(string ipOrDomain);
    }
}