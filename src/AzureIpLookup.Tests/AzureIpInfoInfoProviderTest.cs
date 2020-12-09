using System.Net.Http;
using AzureIpLookup.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace AzureIpLookup.Tests
{
    [TestClass]
    public class AzureIpInfoInfoProviderTest
    {
        private readonly Mock<IHttpClientFactory> mockHttpClientFactory;
        private readonly Mock<ILogger<AzureIpInfoProvider>> mockLogger;

        public AzureIpInfoInfoProviderTest()
        {
            this.mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient();
            this.mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            this.mockLogger = new Mock<ILogger<AzureIpInfoProvider>>();
        }

        [TestMethod]
        public async Task TestLookupPublicAzureIpByDomain()
        {
            var provider = new AzureIpInfoProvider(mockHttpClientFactory.Object, mockLogger.Object);
            var result = await provider.GetAzureIpInfo("portal-prod-southeastasia-02.southeastasia.cloudapp.azure.com");
            Assert.AreEqual("AzureCloud.southeastasia", result.ServiceTagId);
            Assert.AreEqual("52.139.236.115", result.IpAddress);
            Assert.AreEqual("52.139.192.0/18", result.IpAddressPrefix);
            Assert.AreEqual("southeastasia", result.Region);
            Assert.AreEqual("", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupPublicAzureIpByIpAddress()
        {
            var provider = new AzureIpInfoProvider(mockHttpClientFactory.Object, mockLogger.Object);
            var result = await provider.GetAzureIpInfo("52.139.236.115");
            Assert.AreEqual("AzureCloud.southeastasia", result.ServiceTagId);
            Assert.AreEqual("52.139.236.115", result.IpAddress);
            Assert.AreEqual("52.139.192.0/18", result.IpAddressPrefix);
            Assert.AreEqual("southeastasia", result.Region);
            Assert.AreEqual("", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureChinaCloudIpByDomain()
        {
            var provider = new AzureIpInfoProvider(mockHttpClientFactory.Object, mockLogger.Object);
            var result = await provider.GetAzureIpInfo("portal-mc-chinaeast2-02.chinaeast2.cloudapp.chinacloudapi.cn");
            Assert.AreEqual("AzureCloud.chinaeast2", result.ServiceTagId);
            Assert.AreEqual("40.73.108.25", result.IpAddress);
            Assert.AreEqual("40.73.64.0/18", result.IpAddressPrefix);
            Assert.AreEqual("chinaeast2", result.Region);
            Assert.AreEqual("", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureChinaCloudIpByIpAddress()
        {
            var provider = new AzureIpInfoProvider(mockHttpClientFactory.Object, mockLogger.Object);
            var result = await provider.GetAzureIpInfo("40.73.108.25");
            Assert.AreEqual("AzureCloud.chinaeast2", result.ServiceTagId);
            Assert.AreEqual("40.73.108.25", result.IpAddress);
            Assert.AreEqual("40.73.64.0/18", result.IpAddressPrefix);
            Assert.AreEqual("chinaeast2", result.Region);
            Assert.AreEqual("", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureUSGovernmentIpByDomain()
        {
            var provider = new AzureIpInfoProvider(mockHttpClientFactory.Object, mockLogger.Object);
            var result = await provider.GetAzureIpInfo("portal-ff-usgovtexas-02.usgovtexas.cloudapp.usgovcloudapi.net");
            Assert.AreEqual("AzurePortal.USGovTexas", result.ServiceTagId);
            Assert.AreEqual("20.140.57.97", result.IpAddress);
            Assert.AreEqual("20.140.57.96/29", result.IpAddressPrefix);
            Assert.AreEqual("usgovtexas", result.Region);
            Assert.AreEqual("AzurePortal", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureUSGovernmentIpByIpAddress()
        {
            var provider = new AzureIpInfoProvider(mockHttpClientFactory.Object, mockLogger.Object);
            var result = await provider.GetAzureIpInfo("20.140.57.97");
            Assert.AreEqual("AzurePortal.USGovTexas", result.ServiceTagId);
            Assert.AreEqual("20.140.57.97", result.IpAddress);
            Assert.AreEqual("20.140.57.96/29", result.IpAddressPrefix);
            Assert.AreEqual("usgovtexas", result.Region);
            Assert.AreEqual("AzurePortal", result.SystemService);
        }

        [TestMethod]
        public async Task TestLookupInvalidIpAddress()
        {
            var provider = new AzureIpInfoProvider(mockHttpClientFactory.Object, mockLogger.Object);
            var result = await provider.GetAzureIpInfo("1.1.1.1");
            Assert.AreEqual(null, result.ServiceTagId);
            Assert.AreEqual(null, result.IpAddress);
            Assert.AreEqual(null, result.IpAddressPrefix);
            Assert.AreEqual(null, result.Region);
            Assert.AreEqual(null, result.SystemService);
        }
    }
}
