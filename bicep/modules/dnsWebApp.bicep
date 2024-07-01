targetScope = 'resourceGroup'

// Parameters
@description('The dns configuration object')
param parDns object

@description('The web app hostname')
param parWebAppHostname string

@description('The domain verification code')
param parDomainAuthCode string

@description('The tags to apply to the resources.')
param parTags object = resourceGroup().tags

// Existing Resources
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: parDns.Domain
}

// Module Resources
resource dnsCName 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  name: '${parDns.Subdomain}'
  parent: dnsZone

  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: parWebAppHostname
    }
    metadata: parTags
  }
}

resource txtAuth 'Microsoft.Network/dnszones/TXT@2018-05-01' = {
  name: 'asuid.${parDns.Subdomain}'
  parent: dnsZone

  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [parDomainAuthCode]
      }
    ]
    metadata: parTags
  }
}
