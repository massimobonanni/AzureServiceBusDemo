# AzureServiceBusDemo

A .NET 10 sample that demonstrates publishing orders to an Azure Service Bus
topic and processing them independently through two subscriptions. See the
[source guide](src/README.md) for the project architecture, local configuration,
and startup instructions.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- An Azure subscription in which you can create resource groups and Service Bus resources

## Build the solution

```sh
dotnet build src/AzureServiceBus.sln
```

## Deploy to Azure with AZD

The repository uses Bicep through `azure.yaml`. The current AZD configuration
deploys the Azure infrastructure: a resource group, a Standard Service Bus
namespace, `OrdersTopic`, and the `LogisticSubscription` and
`FinanceSubscription` subscriptions.

1. Sign in to Azure:

	```sh
	azd auth login
	```

2. Create and select an AZD environment:

	```sh
	azd env new servicebus-dev
	```

3. Select the Azure region for the resources:

	```sh
	azd env set AZURE_LOCATION westeurope
	```

	Replace `westeurope` with the Azure region you want to use.

4. Provision the resources:

	```sh
	azd up
	```

	Select the target Azure subscription if AZD prompts for it. The command is
	safe to run again to update an existing environment.

5. Verify the non-secret deployment outputs:

	```sh
	azd env get-value SERVICEBUS_FULLYQUALIFIEDNAMESPACE
	azd env get-value SERVICEBUS_TOPICNAME
	azd env get-value SERVICEBUS_LOGISTICSUBSCRIPTIONNAME
	azd env get-value SERVICEBUS_FINANCESUBSCRIPTIONNAME
	```

The Bicep deployment also produces connection strings for local configuration.
Treat those values as secrets and do not commit them. The current `azure.yaml`
does not define application hosting services, so `azd up` provisions the Service
Bus infrastructure but does not publish the three executable projects.

To delete the environment and its Azure resources when you are finished, run:

```sh
azd down
```