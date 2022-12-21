using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Blob;
using AzureIpLookup.Triggers;
using AzureIpLookup.UnitTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AzureIpLookup.UnitTests
{
    [TestClass]
    public class TimerTriggerTest
    {
        private readonly MockLogger<TimerTrigger> mockTimerTriggerLogger;
        private readonly Mock<IHttpClientFactory> mockHttpClientFactory;
        private readonly Mock<IAzureBlobProvider> mockAzureStorageProvider;

        public TimerTriggerTest()
        {
            mockTimerTriggerLogger = new MockLogger<TimerTrigger>();
            mockHttpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            mockAzureStorageProvider = new Mock<IAzureBlobProvider>(MockBehavior.Strict);
            SetupMock();
        }

        [TestMethod]
        public async Task TestSyncServiceTagFilesAsync()
        {
            var trigger = new TimerTrigger(mockTimerTriggerLogger, mockHttpClientFactory.Object, mockAzureStorageProvider.Object);
            await trigger.SyncServiceTagFilesAsync();
            mockAzureStorageProvider.Verify(_ => _.UploadToBlobFromUriAsync(It.IsAny<string>(), It.IsAny<Uri>()), Times.Exactly(4));
        }

        private void SetupMock()
        {
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
            mockAzureStorageProvider.Setup(_ => _.UploadToBlobFromUriAsync(It.IsAny<string>(), It.IsAny<Uri>())).Returns(Task.CompletedTask);
        }
    }
}
