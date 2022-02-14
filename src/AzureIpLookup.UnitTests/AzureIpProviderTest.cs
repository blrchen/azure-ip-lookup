using System.Net.Http;
using System.Threading.Tasks;
using Azure.Ip;
using AzureIpLookup.UnitTests.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AzureIpLookup.UnitTests
{
    [TestClass]
    public class AzureIpProviderTest
    {
        private static string ipAddress = "104.45.231.79";
        private readonly IAzureIpProvider azureIpProvider;
        private readonly Mock<IHttpClientFactory> mockHttpClientFactory;

        public AzureIpProviderTest()
        {
            var mockLogger = new MockLogger<AzureIpProvider>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            this.mockHttpClientFactory = new Mock<IHttpClientFactory>();
            azureIpProvider = new AzureIpProvider(mockLogger, memoryCache, mockHttpClientFactory.Object);
            SetupMock();
        }

        [TestMethod]
        public async Task TestGetAzureIpInfo()
        {
            await azureIpProvider.GetAzureIpInfo(ipAddress);
            ////mockAzureStorageProvider.Verify(_ => _.GetAzureIpInfoListAsync(), Times.Once);
        }

        [TestMethod]
        public async Task TestGetAzureIpInfo_ReadFromCacheIfNoExpired()
        {
            await azureIpProvider.GetAzureIpInfo(ipAddress);
            await azureIpProvider.GetAzureIpInfo(ipAddress);
            ////mockAzureStorageProvider.Verify(_ => _.GetAzureIpInfoListAsync(), Times.Once);
        }

        [TestMethod]
        public void TestParseIpAddress_IpV4()
        {
            const string ip = "104.45.231.79";
            Assert.IsTrue(azureIpProvider.TryParseIpAddress(ip, out string _));
        }

        [TestMethod]
        public void TestParseIpAddress_IpV6()
        {
            const string ip = "2603:1030:0800:0005:0000:0000:BFEE:A418";
            Assert.IsTrue(azureIpProvider.TryParseIpAddress(ip, out string _));
        }

        [TestMethod]
        public void TestParseIpAddress_DomainName()
        {
            const string domainName = "www.azure.com";
            Assert.IsTrue(azureIpProvider.TryParseIpAddress(domainName, out string _));
        }

        [TestMethod]
        public void TestParseIpAddress_Url()
        {
            const string domainName = "http://www.azure.com";
            Assert.IsTrue(azureIpProvider.TryParseIpAddress(domainName, out string _));
        }

        [TestMethod]
        public void TestParseIpAddress_Invalid()
        {
            const string ip = "foo";
            Assert.IsFalse(azureIpProvider.TryParseIpAddress(ip, out string _));
        }

        private void SetupMock()
        {
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        }
    }
}
