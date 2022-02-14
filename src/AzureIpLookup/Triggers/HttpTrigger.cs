using System.Threading.Tasks;
using Azure.Ip;
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
        private readonly IAzureIpProvider azureIpProvider;
        private readonly ILogger<HttpTrigger> logger;

        public HttpTrigger(
            IAzureIpProvider azureIpProvider,
            ILogger<HttpTrigger> logger)
        {
            this.azureIpProvider = azureIpProvider;
            this.logger = logger;
        }

        [FunctionName("GetAzureIpInfo")]
        public async Task<IActionResult> GetAzureIpInfo([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ipinfo")] HttpRequest req)
        {
            string ipOrDomain = req.Query["ipOrDomain"];
            if (string.IsNullOrEmpty(ipOrDomain))
            {
                return new BadRequestObjectResult("ipOrDomain can not be null");
            }

            var result = await azureIpProvider.GetAzureIpInfo(ipOrDomain);

            logger.LogInformation("Function GetAzureIpInfo completed successfully");

            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }
    }
}
