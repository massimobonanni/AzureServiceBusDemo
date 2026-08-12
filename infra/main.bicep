targetScope = 'subscription'

@description('Stable name of the AZD deployment environment.')
@minLength(1)
param environmentName string

@description('Primary Azure region for this deployment.')
param location string

var resourceToken = uniqueString(subscription().id, environmentName)
var abbreviations = loadJsonContent('abbreviations.json')
var resourceGroupName = take('${environmentName}-${abbreviations.resourceGroup}', 50)
var serviceBusName = take('${abbreviations.serviceBusNamespace}-${resourceToken}', 50)

resource deploymentResourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
}

module serviceBus 'modules/servicebus.bicep' = {
  name: 'servicebus'
  scope: deploymentResourceGroup
  params: {
    location: location
    serviceBusName: serviceBusName
  }
}

output SERVICEBUS_FULLYQUALIFIEDNAMESPACE string = serviceBus.outputs.serviceBusFullyQualifiedNamespace
output SERVICEBUS_TOPICNAME string = serviceBus.outputs.serviceBusTopicName
output SERVICEBUS_LOGISTICSUBSCRIPTIONNAME string = serviceBus.outputs.logisticSubscriptionName
output SERVICEBUS_FINANCESUBSCRIPTIONNAME string = serviceBus.outputs.financeSubscriptionName
output SERVICEBUS_TOPIC_CONNECTION_STRING string = serviceBus.outputs.serviceBusTopicConnectionString
output SERVICEBUS_LOGISTIC_SUBSCRIPTION_CONNECTION_STRING string = serviceBus.outputs.logisticSubscriptionConnectionString
output SERVICEBUS_FINANCE_SUBSCRIPTION_CONNECTION_STRING string = serviceBus.outputs.financeSubscriptionConnectionString
