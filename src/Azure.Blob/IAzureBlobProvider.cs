using System.Threading.Tasks;

namespace Azure.Blob
{
    public interface IAzureBlobProvider
    {
        Task UploadToBlobAsync(string blobName, string content);
        Task UploadToBlobFromUrlAsync(string blobName, string url);
    }
}