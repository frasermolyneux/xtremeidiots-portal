resource "azurerm_storage_account" "funcapp-storage-account" {
    name = "funcappsa${var.environment}"
    resource_group_name = azurerm_resource_group.resource-group.name
    location = azurerm_resource_group.resource-group.location
    account_tier = "Standard"
    account_replication_type = "LRS"
}

resource "azurerm_application_insights" "app-insights" {
    name = "XI-Portal-AppInsights-${var.environment}"
    resource_group_name = azurerm_resource_group.resource-group.name
    location = azurerm_resource_group.resource-group.location
    application_type = "web"
}

resource "azurerm_app_service_plan" "funcapp-service-plan" {
    name = "XI-Portal-Func-AppPlan-${var.environment}"
    resource_group_name = azurerm_resource_group.resource-group.name
    location = azurerm_resource_group.resource-group.location
    kind = "FunctionApp"
    sku {
        tier = "Dynamic"
        size = "Y1"
    }
}

resource "azurerm_function_app" "xi-portal-funcapp" {
    name = "XI-Portal-FuncApp-${var.environment}"
    location = azurerm_resource_group.resource-group.location
    resource_group_name = azurerm_resource_group.resource-group.name
    storage_connection_string = azurerm_storage_account.funcapp-storage-account.primary_connection_string
    app_service_plan_id = azurerm_app_service_plan.funcapp-service-plan.id
    version = "~3"
    app_settings {
        "APPINSIGHTS_INSTRUMENTATIONKEY" = "${azurerm_application_insights.app-insights.instrumentation_key}"
    }
    
}