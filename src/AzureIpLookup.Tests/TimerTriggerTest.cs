using System.Threading.Tasks;
using AzureIpLookup.Providers;
using AzureIpLookup.Tests.Common;
using AzureIpLookup.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AzureIpLookup.Tests
{
    [TestClass]
    public class TimerTriggerTest
    {
        private readonly MockLogger<TimerTrigger> mockTimerTriggerLogger;
        private readonly Mock<IAzureStorageProvider> mockAzureStorageProvider;

        public TimerTriggerTest()
        {
            mockTimerTriggerLogger = new MockLogger<TimerTrigger>();
            mockAzureStorageProvider = new Mock<IAzureStorageProvider>(MockBehavior.Strict);
            SetupMock();
        }

        [TestMethod]
        public async Task TestDownloadAzureIpRangeFilesAsync()
        {
            var trigger = new TimerTrigger(mockTimerTriggerLogger, mockAzureStorageProvider.Object);
            await trigger.DownloadAzureIpRangeFilesAsync();

            mockAzureStorageProvider.Verify(provider => provider.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(4));
        }

        private void SetupMock()
        {
            mockAzureStorageProvider.Setup(provider => provider.UploadToBlobAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        }
    }
}
