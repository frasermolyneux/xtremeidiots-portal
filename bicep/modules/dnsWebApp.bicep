targetScope = 'resourceGroup'

// Parameters
@description('The dns configuration object')
param dns object

@description('The web app hostname')
param webAppHostname string

@description('The domain verification code')
param domainAuthCode string

@description('The tags to apply to the resources.')
param tags object = resourceGroup().tags

// Existing Resources
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: dns.Domain
}

// Module Resources
resource dnsCName 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  name: '${dns.Subdomain}'
  parent: dnsZone

  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: webAppHostname
    }
    metadata: tags
  }
}

resource txtAuth 'Microsoft.Network/dnszones/TXT@2018-05-01' = {
  name: 'asuid.${dns.Subdomain}'
  parent: dnsZone

  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [domainAuthCode]
      }
    ]
    metadata: tags
  }
}
