targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
@secure()
param parAdminPassword string
param parKeyVaultName string
param parAdminGroupOid string

// Variables
var varSqlServerName = 'sql-portal-${parEnvironment}-${parLocation}-01'
var varSqlAdminGroupName = 'sg-sql-portal-${parEnvironment}-admins'

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Module Resources
resource adminUsernameSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${varSqlServerName}-admin-username'
  parent: keyVault

  properties: {
    contentType: 'text/plain'
    value: '${varSqlServerName}-addy'
  }
}

resource sqlServer 'Microsoft.Sql/servers@2021-11-01-preview' = {
  name: varSqlServerName
  location: parLocation

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    version: '12.0'

    publicNetworkAccess: 'Enabled'

    administratorLogin: '${varSqlServerName}-addy'
    administratorLoginPassword: parAdminPassword

    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: false
      login: varSqlAdminGroupName
      principalType: 'Group'
      sid: parAdminGroupOid
      tenantId: tenant().tenantId
    }
  }
}

resource allowAzureServicesFirewallRule 'Microsoft.Sql/servers/firewallRules@2021-11-01-preview' = {
  parent: sqlServer
  name: 'allowAzureServicesFirewallRule'

  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

// Outputs
output outSqlServerName string = sqlServer.name
