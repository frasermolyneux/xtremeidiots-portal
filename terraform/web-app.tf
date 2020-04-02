resource "azurerm_resource_group" "resource-group" {
    name = "XI-Portal-Web-${var.environment}"
    location = var.region
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