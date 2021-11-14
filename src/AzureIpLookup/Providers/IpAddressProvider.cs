﻿using System;
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

        public string ParseIpAddress(string ipOrDomain)
        {
            try
            {
                // 1. Parse as ip address (ip v4 or v6)
                if (IPAddress.TryParse(ipOrDomain, out var address))
                {
                    switch (address.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                        case AddressFamily.InterNetworkV6:
                            logger.LogInformation($"{ipOrDomain} is a valid ip address");
                            return ipOrDomain;
                        default:
                            logger.LogInformation($"{ipOrDomain} is can not be resolved to a valid ip address");
                            break;
                    }
                }

                // 2. Parse as hostname
                if (!(ipOrDomain.StartsWith("http://") || ipOrDomain.StartsWith("https://")))
                {
                    ipOrDomain = "http://" + ipOrDomain;
                }

                var tmpUri = new Uri(ipOrDomain);
                ipOrDomain = tmpUri.Host;
                var ipAddresses = Dns.GetHostAddresses(ipOrDomain);
                var ipAddress = ipAddresses[0];
                logger.LogInformation($"{ipOrDomain} is a valid hostname");
                return ipAddress.ToString();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error parsing parse ipOrDomain, error = {ex}");
                return "";
            }
        }
    }
}
