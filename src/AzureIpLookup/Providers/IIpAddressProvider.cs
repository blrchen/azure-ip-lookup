namespace AzureIpLookup.Providers
{
    public interface IIpAddressProvider
    {
        string ParseIpAddress(string ipOrDomain);
    }
}