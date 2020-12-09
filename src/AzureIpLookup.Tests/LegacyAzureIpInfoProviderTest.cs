using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AzureIpLookup.Legacy.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureSpeed.Test
{
    [TestClass]
    public class LegacyAzureIpInfoProviderTest
    {
        private readonly LegacyAzureIpInfoProvider legacyAzureIpInfoProvider;

        public LegacyAzureIpInfoProviderTest()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.legacyAzureIpInfoProvider = new LegacyAzureIpInfoProvider();
        }

        [TestMethod]
        public async Task CanGetRegionNameByIp()
        {
            // ip of azure.com
            string azureComIp = "168.62.225.23";
            var azureComIpInfo = await this.legacyAzureIpInfoProvider.GetLegacyAzureIpInfo(azureComIp);
            Assert.AreEqual("Azure", azureComIpInfo.Cloud);
            Assert.AreEqual("North Central US", azureComIpInfo.Region);
            Assert.AreEqual(azureComIp, azureComIpInfo.IpAddress);

            // ip of azure.cn
            string azureCNIp = "42.159.5.43";
            var azureCNIpInfo = await this.legacyAzureIpInfoProvider.GetLegacyAzureIpInfo(azureCNIp);
            Assert.AreEqual("Azure", azureComIpInfo.Cloud);
            Assert.AreEqual("China North", azureCNIpInfo.Region);
            Assert.AreEqual(azureCNIp, azureCNIpInfo.IpAddress);

            var azureSpeedRegion1 = await this.legacyAzureIpInfoProvider.GetLegacyAzureIpInfo("https://www.azurespeed.com/");
            Assert.AreEqual("Azure", azureComIpInfo.Cloud);
            Assert.AreEqual("East Asia", azureSpeedRegion1.Region);
            Assert.AreEqual("13.94.47.87", azureSpeedRegion1.IpAddress);

            var noRegion = await this.legacyAzureIpInfoProvider.GetLegacyAzureIpInfo("1.1.1.1");
            Assert.IsTrue(string.IsNullOrEmpty(noRegion.Region));
        }
    }
}