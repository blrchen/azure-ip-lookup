using System.Net.Http;
using System.Threading.Tasks;
using AzureIpLookup.DataContracts;
using AzureIpLookup.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace AzureIpLookup.Tests
{
    [TestClass]
    public class ApiClientTests
    {
        private readonly HttpClient httpClient;

        public ApiClientTests()
        {
            httpClient = new HttpClient();
        }

        [TestMethod]
        public async Task TestLookupPublicAzureIpByDomain()
        {
            string ipAddressOrUrl = "portal-prod-southeastasia-02.southeastasia.cloudapp.azure.com";
            var ipInfo = await GetAzureIpInfo(ipAddressOrUrl);
            Assert.AreEqual("AzureCloud.southeastasia", ipInfo.ServiceTagId);
            Assert.AreEqual("52.139.236.115", ipInfo.IpAddress);
            Assert.AreEqual("52.139.192.0/18", ipInfo.IpAddressPrefix);
            Assert.AreEqual("southeastasia", ipInfo.Region);
            Assert.AreEqual("", ipInfo.SystemService);
        }

        [TestMethod]
        public async Task TestLookupPublicAzureIpByIpAddress()
        {
            string ipAddressOrUrl = "52.139.236.115";
            var ipInfo = await GetAzureIpInfo(ipAddressOrUrl);
            Assert.AreEqual("AzureCloud.southeastasia", ipInfo.ServiceTagId);
            Assert.AreEqual("52.139.236.115", ipInfo.IpAddress);
            Assert.AreEqual("52.139.192.0/18", ipInfo.IpAddressPrefix);
            Assert.AreEqual("southeastasia", ipInfo.Region);
            Assert.AreEqual("", ipInfo.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureChinaCloudIpByDomain()
        {
            string ipAddressOrUrl = "portal-mc-chinaeast2-02.chinaeast2.cloudapp.chinacloudapi.cn";
            var ipInfo = await GetAzureIpInfo(ipAddressOrUrl);
            Assert.AreEqual("AzureCloud.chinaeast2", ipInfo.ServiceTagId);
            Assert.AreEqual("40.73.108.25", ipInfo.IpAddress);
            Assert.AreEqual("40.73.64.0/18", ipInfo.IpAddressPrefix);
            Assert.AreEqual("chinaeast2", ipInfo.Region);
            Assert.AreEqual("", ipInfo.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureChinaCloudIpByIpAddress()
        {
            string ipAddressOrUrl = "40.73.108.25";
            var ipInfo = await GetAzureIpInfo(ipAddressOrUrl);
            Assert.AreEqual("AzureCloud.chinaeast2", ipInfo.ServiceTagId);
            Assert.AreEqual("40.73.108.25", ipInfo.IpAddress);
            Assert.AreEqual("40.73.64.0/18", ipInfo.IpAddressPrefix);
            Assert.AreEqual("chinaeast2", ipInfo.Region);
            Assert.AreEqual("", ipInfo.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureUSGovernmentIpByDomain()
        {
            string ipAddressOrUrl = "portal-ff-usgovtexas-02.usgovtexas.cloudapp.usgovcloudapi.net";
            var ipInfo = await GetAzureIpInfo(ipAddressOrUrl);
            Assert.AreEqual("AzurePortal.USGovTexas", ipInfo.ServiceTagId);
            Assert.AreEqual("20.140.57.97", ipInfo.IpAddress);
            Assert.AreEqual("20.140.57.96/29", ipInfo.IpAddressPrefix);
            Assert.AreEqual("usgovtexas", ipInfo.Region);
            Assert.AreEqual("AzurePortal", ipInfo.SystemService);
        }

        [TestMethod]
        public async Task TestLookupAzureUSGovernmentIpByIpAddress()
        {
            string ipAddressOrUrl = "20.140.57.97";
            var ipInfo = await GetAzureIpInfo(ipAddressOrUrl);
            Assert.AreEqual("AzurePortal.USGovTexas", ipInfo.ServiceTagId);
            Assert.AreEqual("20.140.57.97", ipInfo.IpAddress);
            Assert.AreEqual("20.140.57.96/29", ipInfo.IpAddressPrefix);
            Assert.AreEqual("usgovtexas", ipInfo.Region);
            Assert.AreEqual("AzurePortal", ipInfo.SystemService);
        }

        [TestMethod]
        public async Task TestLookupInvalidIpAddress()
        {
            string ipAddressOrUrl = "1.1.1.1";
            var ipInfo = await GetAzureIpInfo(ipAddressOrUrl);
            Assert.AreEqual(null, ipInfo.ServiceTagId);
            Assert.AreEqual(null, ipInfo.IpAddress);
            Assert.AreEqual(null, ipInfo.IpAddressPrefix);
            Assert.AreEqual(null, ipInfo.Region);
            Assert.AreEqual(null, ipInfo.SystemService);
        }

        private async Task<AzureIpInfo> GetAzureIpInfo(string ipAddressOrUrl)
        {
            string url = $"https://www.azurespeed.com/api/ipinfo?ipAddressOrUrl={ipAddressOrUrl}";
            var response = await httpClient.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            var ipInfo = JsonConvert.DeserializeObject<AzureIpInfo>(content);
            return ipInfo;
        }
    }
}
