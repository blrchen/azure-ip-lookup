# azure-ip-lookup

Web APIs to lookup Azure region info and service tag based on domain name or ip address

## Demo

### Web UI

* <https://www.azurespeed.com/Azure/IPLookup>

### Web API

| Use case               | Live Example                                                                                          |
|------------------------|-------------------------------------------------------------------------------------------------------|
| Lookup by domain name  | <https://azureiplookup.azurewebsites.net/api/ipinfo?ipOrDomain=www.azurespeed.com>                      |
| Lookup by IPv4 address | <https://azureiplookup.azurewebsites.net/api/ipinfo?ipOrDomain=104.45.231.79>                           |
| Lookup by IPv6 address | <https://azureiplookup.azurewebsites.net/api/ipinfo?ipOrDomain=2603:1030:0800:0005:0000:0000:BFEE:A418> |

## Local development env setup steps

1. Install dotnet core sdk 6
2. Run Azurite Emulator locally with following docker command. 

    ```bash
    docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 \
        mcr.microsoft.com/azure-storage/azurite
    ```

3. Open **AzureIpLookup.sln** in Visual Studio 2022 or Rider
4. Compile the code and run locally, you should be able to access local endpoint <http://localhost:7071/api/ipinfo?ipOrDomain=40.78.234.177> if everything runs correctly

## Cloud deployment

Currently only Azure Function deployment is supported, please check Azure function official doc site for detail instructions.

## Resources

* Service Tags: <https://docs.microsoft.com/en-us/azure/virtual-network/security-overview#service-tags>
* Azure's IP range and service tag data download urls:
  * Azure Cloud: <https://www.microsoft.com/en-us/download/details.aspx?id=56519>
  * Azure China Cloud: <https://www.microsoft.com/en-us/download/details.aspx?id=57062>
  * Azure US Government Cloud: <https://www.microsoft.com/en-us/download/details.aspx?id=57063>
  * Azure Germany Cloud: <https://www.microsoft.com/en-us/download/details.aspx?id=57064>
