using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureIpLookup.DataContracts;
using AzureIpLookup.Providers;
using AzureIpLookup.Tests.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace AzureIpLookup.Tests
{
    [TestClass]
    public class AzureIpInfoInfoProviderTest
    {
        private readonly Mock<IAzureStorageProvider> mockAzureStorageProvider;
        private readonly IAzureIpInfoProvider azureIpInfoProvider;

        public AzureIpInfoInfoProviderTest()
        {
            mockAzureStorageProvider = new Mock<IAzureStorageProvider>(MockBehavior.Strict);
            var mockLogger = new MockLogger<AzureIpInfoProvider>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            azureIpInfoProvider = new AzureIpInfoProvider(mockAzureStorageProvider.Object, mockLogger, memoryCache);
            SetupMock();
        }

        [TestMethod]
        public async Task TestGetAzureIpInfo()
        {
            await azureIpInfoProvider.GetAzureIpInfo("1.1.1.1");
            mockAzureStorageProvider.Verify(p => p.GetAzureIpInfoListAsync(), Times.Once);
        }

        [TestMethod]
        public async Task TestGetAzureIpInfo_ReadFromCacheIfNoExpired()
        {
            await azureIpInfoProvider.GetAzureIpInfo("1.1.1.1");
            await azureIpInfoProvider.GetAzureIpInfo("1.1.1.1");
            mockAzureStorageProvider.Verify(p => p.GetAzureIpInfoListAsync(), Times.Once);
        }

        private void SetupMock()
        {
            var azureIpInfoListFileContent = File.ReadAllText(@"Data\azureIpInfoList.json");
            var azureIpInfoList = JsonConvert.DeserializeObject<IList<AzureIpInfo>>(azureIpInfoListFileContent);
            mockAzureStorageProvider.Setup(provider => provider.GetAzureIpInfoListAsync()).ReturnsAsync(azureIpInfoList);
        }
    }
}
