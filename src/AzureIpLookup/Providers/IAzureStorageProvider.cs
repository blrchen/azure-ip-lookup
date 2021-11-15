using System.Collections.Generic;
using System.Threading.Tasks;
using AzureIpLookup.DataContracts;

namespace AzureIpLookup.Providers
{
    public interface IAzureStorageProvider
    {
        Task UploadBlobAsync(string blobName, string content);
        Task UploadBlobFromUrlAsync(string blobName, string downloadUrl);
        Task<IList<AzureIpInfo>> GetAzureIpInfoListAsync();
    }
}