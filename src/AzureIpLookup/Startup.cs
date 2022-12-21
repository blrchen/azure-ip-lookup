using System;
using Azure.Blob;
using Azure.Ip;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureIpLookup.Startup))]
namespace AzureIpLookup
{
    // This class is used for dependency injection setup when the functions runtime starts up.
    // Details see https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddLogging();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton(_ =>
            {
                string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                const string blobContainerName = "ipfiles";
                return new BlobContainerClient(storageConnectionString, blobContainerName);
            });
            builder.Services.AddSingleton<IAzureIpProvider, AzureIpProvider>();
            builder.Services.AddSingleton<IAzureBlobProvider, AzureBlobProvider>();
        }
    }
}
