using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace AzureIpLookup.Providers
{
    public class IpAddressProvider : IIpAddressProvider
    {
        private readonly ILogger<IpAddressProvider> logger;

        public IpAddressProvider(ILogger<IpAddressProvider> logger)
        {
            this.logger = logger;
        }

        public bool TryParseIpAddress(string ipOrDomain, out string result)
        {
            try
            {
                // 1. If input is an ip v4 or v6 address, skip parse
                if (IPAddress.TryParse(ipOrDomain, out var address))
                {
                    switch (address.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                        case AddressFamily.InterNetworkV6:
                            result = ipOrDomain;
                            logger.LogInformation($"{ipOrDomain} is a valid ip address, parse is skipped");
                            return true;
                        default:
                            // Not a valid ip address, do nothing
                            break;
                    }
                }

                // 2. Convert to url and parse
                if (!(ipOrDomain.StartsWith("http://") || ipOrDomain.StartsWith("https://")))
                {
                    ipOrDomain = "http://" + ipOrDomain;
                }

                var tmpUri = new Uri(ipOrDomain);
                ipOrDomain = tmpUri.Host;
                var ipAddresses = Dns.GetHostAddresses(ipOrDomain);
                var ipAddress = ipAddresses[0];
                result = ipAddress.ToString();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error parsing ipOrDomain, error = {ex}");
                result = string.Empty;
                return false;
            }
        }
    }
}
