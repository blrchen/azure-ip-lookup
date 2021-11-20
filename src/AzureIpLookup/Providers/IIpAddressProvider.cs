namespace AzureIpLookup.Providers
{
    public interface IIpAddressProvider
    {
        bool TryParseIpAddress(string ipOrDomain, out string result);
    }
}