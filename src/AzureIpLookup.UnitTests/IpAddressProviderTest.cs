using AzureIpLookup.Providers;
using AzureIpLookup.UnitTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureIpLookup.UnitTests
{
    [TestClass]
    public class IpAddressProviderTest
    {
        private readonly IIpAddressProvider ipAddressProvider;

        public IpAddressProviderTest()
        {
            var mockLogger = new MockLogger<IpAddressProvider>();
            ipAddressProvider = new IpAddressProvider(mockLogger);
        }

        [TestMethod]
        public void TestParseIpAddress_IpV4()
        {
            const string ip = "104.45.231.79";
            Assert.IsTrue(ipAddressProvider.TryParseIpAddress(ip, out string _));
        }

        [TestMethod]
        public void TestParseIpAddress_IpV6()
        {
            const string ip = "2603:1030:0800:0005:0000:0000:BFEE:A418";
            Assert.IsTrue(ipAddressProvider.TryParseIpAddress(ip, out string _));
        }

        [TestMethod]
        public void TestParseIpAddress_DomainName()
        {
            const string domainName = "www.azure.com";
            Assert.IsTrue(ipAddressProvider.TryParseIpAddress(domainName, out string _));
        }

        [TestMethod]
        public void TestParseIpAddress_Url()
        {
            const string domainName = "http://www.azure.com";
            Assert.IsTrue(ipAddressProvider.TryParseIpAddress(domainName, out string _));
        }

        [TestMethod]
        public void TestParseIpAddress_Invalid()
        {
            const string ip = "foo";
            Assert.IsFalse(ipAddressProvider.TryParseIpAddress(ip, out string _));
        }
    }
}
