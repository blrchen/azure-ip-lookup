using System.Threading.Tasks;
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
        private readonly ILogger<HttpTrigger> logger;

        public HttpTrigger(
            IAzureIpInfoProvider azureIpInfoProvider,
            ILogger<HttpTrigger> logger)
        {
            this.azureIpInfoProvider = azureIpInfoProvider;
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

            logger.LogInformation("Function GetAzureIpInfo completed successfully");

            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }
    }
}
