using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureIpLookup.DataContracts;
using AzureIpLookup.Providers;
using AzureIpLookup.UnitTests.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace AzureIpLookup.UnitTests
{
    [TestClass]
    public class AzureIpInfoInfoProviderTest
    {
        private const string IpAddress = "104.45.231.79";
        private readonly Mock<IAzureStorageProvider> mockAzureStorageProvider;
        private readonly Mock<IIpAddressProvider> mockIpAddressProvider;
        private readonly IAzureIpInfoProvider azureIpInfoProvider;

        public AzureIpInfoInfoProviderTest()
        {
            mockAzureStorageProvider = new Mock<IAzureStorageProvider>(MockBehavior.Strict);
            mockIpAddressProvider = new Mock<IIpAddressProvider>(MockBehavior.Strict);
            var mockLogger = new MockLogger<AzureIpInfoProvider>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            azureIpInfoProvider = new AzureIpInfoProvider(mockAzureStorageProvider.Object, mockIpAddressProvider.Object, mockLogger, memoryCache);
            SetupMock();
        }

        [TestMethod]
        public async Task TestGetAzureIpInfo()
        {
            await azureIpInfoProvider.GetAzureIpInfo(IpAddress);
            mockAzureStorageProvider.Verify(_ => _.GetAzureIpInfoListAsync(), Times.Once);
        }

        [TestMethod]
        public async Task TestGetAzureIpInfo_ReadFromCacheIfNoExpired()
        {
            await azureIpInfoProvider.GetAzureIpInfo(IpAddress);
            await azureIpInfoProvider.GetAzureIpInfo(IpAddress);
            mockAzureStorageProvider.Verify(_ => _.GetAzureIpInfoListAsync(), Times.Once);
        }

        private void SetupMock()
        {
            var azureIpInfoListFileContent = File.ReadAllText(@"Data\azureIpInfoList.json");
            var azureIpInfoList = JsonConvert.DeserializeObject<IList<AzureIpInfo>>(azureIpInfoListFileContent);
            mockAzureStorageProvider.Setup(_ => _.GetAzureIpInfoListAsync()).ReturnsAsync(azureIpInfoList);
            mockIpAddressProvider.Setup(_ => _.ParseIpAddress(It.IsAny<string>())).Returns(IpAddress);
        }
    }
}
