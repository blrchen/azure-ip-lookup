using System.Threading.Tasks;
using AzureIpLookup.Legacy.Contracts;

namespace AzureIpLookup.Legacy.Providers
{
    public interface ILegacyAzureIpInfoProvider
    {
        Task<LegacyAzureIpInfo> GetLegacyAzureIpInfo(string ipAddressOrUrl);
    }
}
