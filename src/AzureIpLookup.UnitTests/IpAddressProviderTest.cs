using System.Threading.Tasks;
using AzureIpLookup.Providers;
using AzureIpLookup.UnitTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            string result = ipAddressProvider.ParseIpAddress(ip);
            Assert.AreEqual(ip, result);
        }

        [TestMethod]
        public void TestParseIpAddress_IpV6()
        {
            const string ip = "2603:1030:0800:0005:0000:0000:BFEE:A418";
            string result = ipAddressProvider.ParseIpAddress(ip);
            Assert.AreEqual(ip, result);
        }

        [TestMethod]
        public void TestParseIpAddress_DomainName()
        {
            const string domain = "2603:1030:0800:0005:0000:0000:BFEE:A418";
            string result = ipAddressProvider.ParseIpAddress(domain);
            Assert.AreEqual(domain, result);
        }

        [TestMethod]
        public void TestParseIpAddress_Invalid()
        {
            const string ip = "1.1.1.1";
            string result = ipAddressProvider.ParseIpAddress(ip);
            Assert.AreEqual(ip, result);
        }
    }
}
