using System.IO;
using System.Threading.Tasks;
using AzureIpLookup.Legacy.Providers;
using AzureIpLookup.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureIpLookup.Triggers
{
    public class HttpTrigger
    {
        private readonly IAzureIpInfoProvider azureIpInfoProvider;
        private readonly ILegacyAzureIpInfoProvider legacyAzureIpInfoProvider;
        private readonly ILogger<HttpTrigger> logger;

        public HttpTrigger(
            IAzureIpInfoProvider azureIpInfoProvider,
            ILegacyAzureIpInfoProvider legacyAzureIpInfoProvider,
            ILogger<HttpTrigger> logger)
        {
            this.azureIpInfoProvider = azureIpInfoProvider;
            this.legacyAzureIpInfoProvider = legacyAzureIpInfoProvider;
            this.logger = logger;
        }

        [FunctionName("GetAzureIpInfo")]
        public async Task<IActionResult> GetAzureIpInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ipinfo")] HttpRequest req)
        {
            logger.LogInformation("Function GetAzureIpInfo starts");

            string ipOrDomain = req.Query["ipOrDomain"];
            if (string.IsNullOrEmpty(ipOrDomain))
            {
                return new BadRequestObjectResult("ipOrDomain can not be null");
            }

            var result = await azureIpInfoProvider.GetAzureIpInfo(ipOrDomain);
            string jsonResult = JsonConvert.SerializeObject(result);
            logger.LogInformation("Function GetAzureIpInfo completes successfully");
            logger.LogInformation(jsonResult);

            return new OkObjectResult(jsonResult);
        }

        [FunctionName("GetLegacyAzureIpInfo")]
        public async Task<IActionResult> GetLegacyAzureIpInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "legacyipinfo")] HttpRequest req)
        {
            logger.LogInformation("Function GetLegacyAzureIpInfo starts");

            string ipOrDomain = req.Query["ipOrDomain"];
            if (string.IsNullOrEmpty(ipOrDomain))
            {
                return new BadRequestObjectResult("ipOrDomain can not be null");
            }

            var result = await this.legacyAzureIpInfoProvider.GetLegacyAzureIpInfo(ipOrDomain);
            string jsonResult = JsonConvert.SerializeObject(result);
            logger.LogInformation("Function GetAzureIpInfo completes successfully");
            logger.LogInformation(jsonResult);

            return new OkObjectResult(jsonResult);
        }
    }
}
