using System;
using System.Net;

namespace AzureIpLookup.Legacy.Common
{
    public static class Utils
    {
        public static string ConvertToIpAddress(string ipAddressOrUrl)
        {
            if (string.IsNullOrEmpty(ipAddressOrUrl))
            {
                throw new Exception("ipAddressOrUrl can not be null");
            }

            if (!(ipAddressOrUrl.StartsWith("http://") || ipAddressOrUrl.StartsWith("https://")))
            {
                ipAddressOrUrl = "http://" + ipAddressOrUrl;
            }

            var tmpUri = new Uri(ipAddressOrUrl);
            ipAddressOrUrl = tmpUri.Host;
            var ipAddresses = Dns.GetHostAddresses(ipAddressOrUrl);

            var ipAddress = ipAddresses[0];
            return ipAddress.ToString();
        }
    }
}
