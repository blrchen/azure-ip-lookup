# Azure IP Lookup

This tool provides API services to fetch Azure region data and service tags via a domain name or an IP address.

## Web UI

* Visit the Web UI here: <https://www.azurespeed.com/Azure/IPLookup>

## Web API Usage

| Function          | Example URL                                                                                          |
|-------------------|-------------------------------------------------------------------------------------------------------|
| Domain name lookup  | <https://azure-ip-lookup.azurewebsites.net/api/ipAddress?ipOrDomain=www.azurespeed.com>                      |
| IPv4 address lookup | <https://azure-ip-lookup.azurewebsites.net/api/ipAddress?ipOrDomain=104.45.231.79>                           |
| IPv6 address lookup | <https://azure-ip-lookup.azurewebsites.net/api/ipAddress?ipOrDomain=2603:1030:0800:0005:0000:0000:BFEE:A418> |

## Local Development Environment Setup

1. Download and install Dotnet Core SDK 6 from <https://dotnet.microsoft.com/download/dotnet/6.0>.
2. Open **AzureIpLookup.sln** file in Visual Studio 2022 or Rider.
3. Compile the code and start the function app locally. Access the local endpoint at <http://localhost:7071/api/ipaddress?ipOrDomain=40.78.234.177>. If you encounter an error `The listener for function 'SyncServiceTagFiles' was unable to start,` try running the Azurite Emulator locally with the following Docker command and then restart the function app:

    ```bash
    docker run --name azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
    ```

## Cloud Deployment

At the moment, deployment is only supported on Azure Function. For detailed instructions, refer to the official Azure Function documentation.

## Additional Resources

* Service Tags: <https://docs.microsoft.com/en-us/azure/virtual-network/security-overview#service-tags>
* URLs to download Azure's IP range and service tag data:
  * Azure Cloud: <https://www.microsoft.com/en-us/download/details.aspx?id=56519>
  * Azure China Cloud: <https://www.microsoft.com/en-us/download/details.aspx?id=57062>
  * Azure US Government Cloud: <https://www.microsoft.com/en-us/download/details.aspx?id=57063>
