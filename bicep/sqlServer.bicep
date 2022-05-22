targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
@secure()
param parAdminPassword string
param parKeyVaultName string
param parAdminGroupName string
param parAdminGroupOid string

// Variables
var varSqlServerName = 'sql-portal-${parEnvironment}-${parLocation}-01'

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
      login: parAdminGroupName
      principalType: 'Group'
      sid: parAdminGroupOid
      tenantId: tenant().tenantId
    }
  }
}

resource portalDatabase 'Microsoft.Sql/servers/databases@2021-11-01-preview' = {
  parent: sqlServer
  name: 'portaldb'
  location: parLocation

  sku: {
    capacity: 10
    name: 'Standard'
    tier: 'Standard'
  }

  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 21474836480
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Zone'
  }
}

resource identityDatabase 'Microsoft.Sql/servers/databases@2021-11-01-preview' = {
  parent: sqlServer
  name: 'identitydb'
  location: parLocation

  sku: {
    capacity: 5
    name: 'Basic'
    tier: 'Basic'
  }

  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Zone'
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
