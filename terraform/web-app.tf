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

resource "azurerm_app_service_plan" "app-service-plan" {
    name = "XI-Portal-Web-AppPlan-${var.environment}"
    resource_group_name = azurerm_resource_group.resource-group.name
    location = azurerm_resource_group.resource-group.location

    sku {
        tier = "Basic"
        size = "B1"
    }
}

resource "azurerm_app_service" "app-service" {
  name = "XI-Portal-WebApp-${var.environment}"
  location = var.region
  resource_group_name = azurerm_resource_group.resource-group.name
  app_service_plan_id = azurerm_app_service_plan.app-service-plan.id
}

resource "cloudflare_record" "frontend-dns" {
  zone_id = var.hostname_zone_id
  name = var.hostname
  value = azurerm_app_service.app-service.default_site_hostname
  type = "CNAME"
  proxied = false
}

#This is buggy as shit. Can do it manually through portal.azure.com but fails through here.

#resource "azurerm_app_service_custom_hostname_binding" "custom-hostname" {
#  hostname = var.environment
#  app_service_name = azurerm_app_service.app-service.name
#  resource_group_name = azurerm_resource_group.resource-group.name
#}