param environmentName string
param location string = resourceGroup().location

var resourceToken = uniqueString(subscription().id, resourceGroup().id, environmentName)
var serviceBusName = take('sb-${resourceToken}', 50)
var containerEnvironmentName = 'cae-${resourceToken}'
var containerAppName = 'ca-${resourceToken}'
var logAnalyticsName = 'log-${resourceToken}'

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

resource messagesQueue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: serviceBusNamespace
  name: 'messages'
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: containerEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'app'
          image: 'mcr.microsoft.com/k8se/quickstart:latest'
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

output AZURE_CONTAINER_APP_ENDPOINT string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output SERVICEBUS_FULLYQUALIFIEDNAMESPACE string = '${serviceBusNamespace.name}.servicebus.windows.net'
output SERVICEBUS_QUEUENAME string = messagesQueue.name
