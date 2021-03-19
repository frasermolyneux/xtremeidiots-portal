provider "azurerm" { 
    version = "~> 2.52.0"
    features {}
}

terraform {
    backend "azurerm" {}
}