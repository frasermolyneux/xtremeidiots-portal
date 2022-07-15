targetScope = 'resourceGroup'

// Parameters
param parServiceBusName string

// Existing In-Scope Resources
resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: parServiceBusName
}

// Module Resources
resource playerConnectedServiceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  name: 'player_connected_queue'
  parent: serviceBus

  properties: {}
}

resource chatMessageServiceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  name: 'chat_message_queue'
  parent: serviceBus

  properties: {}
}

resource mapVoteServiceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  name: 'map_vote_queue'
  parent: serviceBus

  properties: {}
}

resource serverConnectedServiceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  name: 'server_connected_queue'
  parent: serviceBus

  properties: {}
}

resource mapChangeServiceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  name: 'map_change_queue'
  parent: serviceBus

  properties: {}
}
