resource "azurerm_resource_group" "resource-group" {
    name = "XI-Portal-Web-${var.environment}"
    location = var.region
}

resource "azurerm_storage_account" "identity-storage" {
    name = "identitysa${var.environment}"
    resource_group_name = azurerm_resource_group.resource-group.name
    location = azurerm_resource_group.resource-group.location
    account_tier = "Standard"
    account_replication_type = "LRS"
}

output "identity_storage_connection" {
  value = azurerm_storage_account.identity-storage.primary_connection_string
  sensitive = true
}

resource "azurerm_storage_account" "appdata-storage" {
    name = "appdata${var.environment}"
    resource_group_name = azurerm_resource_group.resource-group.name
    location = azurerm_resource_group.resource-group.location
    account_tier = "Standard"
    account_replication_type = "LRS"
}

output "appdata_storage_connection" {
  value = azurerm_storage_account.appdata-storage.primary_connection_string
  sensitive = true
}

resource "azurerm_app_service_plan" "app-service-plan" {
    name = "XI-Portal-Web-AppPlan-${var.environment}"
    resource_group_name = azurerm_resource_group.resource-group.name
    location = azurerm_resource_group.resource-group.location

    sku {
        tier = "Shared"
        size = "D1"
    }
}

resource "azurerm_app_service" "app-service" {
  name = "XI-Portal-WebApp-${var.environment}"
  location = var.region
  resource_group_name = azurerm_resource_group.resource-group.name
  app_service_plan_id = azurerm_app_service_plan.app-service-plan.id
}