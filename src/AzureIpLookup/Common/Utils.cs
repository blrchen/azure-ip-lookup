using System;
using System.Net;

namespace AzureIpLookup.Common
{
    public static class Utils
    {
        public static string ConvertToIpAddress(string ipOrDomain)
        {
            if (string.IsNullOrEmpty(ipOrDomain))
            {
                throw new Exception("ipOrDomain can not be null");
            }

            // Ensure valid uri format
            if (!(ipOrDomain.StartsWith("http://") || ipOrDomain.StartsWith("https://")))
            {
                ipOrDomain = "http://" + ipOrDomain;
            }

            var tmpUri = new Uri(ipOrDomain);
            ipOrDomain = tmpUri.Host;
            IPAddress[] ipAddresses = Dns.GetHostAddresses(ipOrDomain);
            var ipAddress = ipAddresses[0];
            return ipAddress.ToString();
        }
    }
}
