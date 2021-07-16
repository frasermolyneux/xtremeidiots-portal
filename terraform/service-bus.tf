resource "azurerm_servicebus_namespace" "servicebus" {
  name = "xi-portal-sbns-${var.environment}"
  location = azurerm_resource_group.resource-group.location
  resource_group_name = azurerm_resource_group.resource-group.name
  sku = "Basic"
}

resource "azurerm_servicebus_queue" "servicebus-queue" {
  name = "map-votes"
  resource_group_name = azurerm_resource_group.resource-group.name
  namespace_name = azurerm_servicebus_namespace.servicebus.name

  enable_partitioning = true
}

resource "azurerm_eventhub_namespace_authorization_rule" "servicebus-function-authrule" {
  name                = "function-authrule"
  namespace_name      = azurerm_servicebus_namespace.servicebus.name
  queue_name          = azurerm_servicebus_queue.servicebus-queue.name
  resource_group_name = azurerm_resource_group.resource-group.name

  listen = true
  send   = true
  manage = true
}

output "servicebus_connection_string" {
  value = azurerm_application_insights.servicebus-function-authrule.primary_connection_string
}

resource "azurerm_eventhub_namespace_authorization_rule" "servicebus-bot-authrule" {
  name                = "bot-authrule"
  namespace_name      = azurerm_servicebus_namespace.servicebus.name
  queue_name          = azurerm_servicebus_queue.servicebus-queue.name
  resource_group_name = azurerm_resource_group.resource-group.name

  listen = false
  send   = true
  manage = false
}