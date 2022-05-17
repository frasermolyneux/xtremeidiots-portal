provider "azurerm" { 
    version = "~> 2.71.0"
    features {}
}

terraform {
    backend "azurerm" {}
}