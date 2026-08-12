# AzureServiceBusDemo

An Azure Developer CLI (AZD) starter for a .NET web application and an Azure
Service Bus queue.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- An Azure subscription

## Run locally

```sh
dotnet run --project src/AzureServiceBusDemo
```

The application exposes `GET /health` and `GET /servicebus`.

## Provision and deploy

Authenticate with Azure, create an environment, then provision and deploy:

```sh
azd auth login
azd up
```

`azd up` provisions a Container Apps environment, the web application, and a
Service Bus namespace with the `messages` queue.