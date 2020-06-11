provider "azurerm" { 
    version = "~> 2.3.0"
    features {}
}

provider "cloudflare" {
  version = "~> 2.0"
  api_token = var.cloudflare_api_token
}

terraform {
    backend "azurerm" {}
}