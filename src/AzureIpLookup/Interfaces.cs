using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureIpLookup
{
    public interface IAzureStorageClient
    {
        Task UploadToBlobFromUri(string blobName, Uri uri);
    }

    public interface IAzureIpAddressService
    {
        Task<IList<AzureIpAddress>> GetAzureIpAddressList(string ipOrDomain);

        // TODO: TryParseIpAddress is only consumed by ut, can be converted to internal
        public bool TryParseIpAddress(string ipOrDomain, out string result);
    }
}
