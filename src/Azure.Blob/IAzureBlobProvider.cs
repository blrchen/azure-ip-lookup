using System;
using System.Threading.Tasks;

namespace Azure.Blob
{
    public interface IAzureBlobProvider
    {
        Task UploadToBlobFromUriAsync(string blobName, Uri uri);
    }
}