# ProxyCheck.io Integration Guide

## Overview

The admin-webapp has been enhanced with ProxyCheck.io integration to provide risk assessments for IP addresses displayed throughout the application. This feature displays a color-coded risk score, and status pills indicating VPN and proxy detection.

## Features

- Color-coded risk score based on ProxyCheck.io risk assessment (0-100)
- Visual indicators for VPN detection
- Visual indicators for proxy detection
- Memory caching to reduce API calls
- Integration with Azure Key Vault for secure API key storage

## Configuration

### ProxyCheck API Key

The ProxyCheck API key should be stored securely in Azure Key Vault and injected into the application settings. Follow these steps to configure the key:

1. Sign up for an account at [ProxyCheck.io](https://proxycheck.io/)
2. Obtain your API key from the dashboard
3. Add the API key to your Azure Key Vault

### Azure Key Vault Configuration

The ProxyCheck API key should be stored in Azure Key Vault and referenced in the app service configuration.

#### 1. Add the Secret to Key Vault

Assuming you already have Key Vault configured for your environment, add the ProxyCheck API key:

```bash
# Using Azure CLI
az keyvault secret set --vault-name <your-keyvault-name> --name "ProxyCheck--ApiKey" --value "<your-proxycheck-api-key>"
```

#### 2. Update App Service Configuration

The app service should reference the Key Vault secret using a reference string:

```bash
# Using Azure CLI
az webapp config appsettings set --resource-group <resource-group> --name <app-service-name> --settings "ProxyCheck:ApiKey=@Microsoft.KeyVault(SecretUri=https://<your-keyvault>.vault.azure.net/secrets/ProxyCheck--ApiKey)"
```

### Local Development

For local development, add the ProxyCheck API key to your `appsettings.Development.json` file:

```json
{
  "ProxyCheck": {
    "ApiKey": "your-api-key-here"
  }
}
```

## Usage

The ProxyCheck integration will automatically enhance IP address displays throughout the admin-webapp. The following views have been updated to display ProxyCheck data:

1. IP Search view (`/Players/IpIndex`)
2. RCON view (`/ServerAdmin/ViewRcon`)

Each IP address display includes:

- The IP address itself, linking to the ProxyCheck.io API
- A color-coded pill indicating the risk score
  - Green (0-24): Low risk
  - Blue (25-49): Medium-low risk
  - Yellow (50-79): Medium-high risk
  - Red (80-100): High risk
- A red "Proxy" pill if the IP is detected as a proxy
- A yellow "VPN" pill if the IP is detected as a VPN

## Implementation Details

### Caching

ProxyCheck results are cached in memory for 1 hour to reduce API calls and improve performance. The cache key is based on the IP address.

### Service Architecture

- `ProxyCheckService`: Core service for interacting with the ProxyCheck.io API
- `ProxyCheckResult`: Model representing the API response
- `ProxyCheckExtensions`: Extension methods to enrich player data with ProxyCheck information
- `PlayerDtoExtensions`: Extension methods to add ProxyCheck properties to player DTOs

### JavaScript Helpers

The `proxyCheckIpLink` function in site.js formats IP addresses with risk scores and status pills.
