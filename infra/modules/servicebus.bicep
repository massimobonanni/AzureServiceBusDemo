param location string
param serviceBusName string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: serviceBusName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource ordersTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: serviceBusNamespace
  name: 'OrdersTopic'
}

resource logisticSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
  parent: ordersTopic
  name: 'LogisticSubscription'
}

resource financeSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
  parent: ordersTopic
  name: 'FinanceSubscription'
}

resource topicPublisherPolicy 'Microsoft.ServiceBus/namespaces/topics/authorizationRules@2024-01-01' = {
  parent: ordersTopic
  name: 'publisher'
  properties: {
    rights: [
      'Send'
    ]
  }
}

resource topicReceiverPolicy 'Microsoft.ServiceBus/namespaces/topics/authorizationRules@2024-01-01' = {
  parent: ordersTopic
  name: 'receiver'
  properties: {
    rights: [
      'Listen'
    ]
  }
}

output serviceBusFullyQualifiedNamespace string = '${serviceBusNamespace.name}.servicebus.windows.net'
output serviceBusTopicName string = ordersTopic.name
output logisticSubscriptionName string = logisticSubscription.name
output financeSubscriptionName string = financeSubscription.name
output serviceBusTopicConnectionString string = topicPublisherPolicy.listKeys().primaryConnectionString
output logisticSubscriptionConnectionString string = topicReceiverPolicy.listKeys().primaryConnectionString
output financeSubscriptionConnectionString string =topicReceiverPolicy.listKeys().primaryConnectionString
