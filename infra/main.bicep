param environmentName string
param location string = resourceGroup().location

var resourceToken = uniqueString(subscription().id, resourceGroup().id, environmentName)
var serviceBusName = take('sb-${resourceToken}', 50)
var containerEnvironmentName = 'cae-${resourceToken}'
var containerAppName = 'ca-${resourceToken}'
var logAnalyticsName = 'log-${resourceToken}'
var containerRegistryName = 'acr${resourceToken}'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource containerEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerEnvironmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: serviceBusName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

resource messagesQueue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: serviceBusNamespace
  name: 'messages'
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerEnvironment.id
    configuration: {
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: 'system'
        }
      ]
      ingress: {
        external: true
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'app'
          image: '${containerRegistry.properties.loginServer}/${containerAppName}:latest'
          env: [
            {
              name: 'SERVICEBUS__FULLYQUALIFIEDNAMESPACE'
              value: '${serviceBusNamespace.name}.servicebus.windows.net'
            }
            {
              name: 'SERVICEBUS__QUEUENAME'
              value: messagesQueue.name
            }
          ]
        }
      ]
    }
  }
}

resource containerRegistryPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, containerApp.id, 'AcrPull')
  scope: containerRegistry
  properties: {
    principalId: containerApp.identity.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
  }
}

output AZURE_CONTAINER_APP_ENDPOINT string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.properties.loginServer
output SERVICEBUS_FULLYQUALIFIEDNAMESPACE string = '${serviceBusNamespace.name}.servicebus.windows.net'
output SERVICEBUS_QUEUENAME string = messagesQueue.name
