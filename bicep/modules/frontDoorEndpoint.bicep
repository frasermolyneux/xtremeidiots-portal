targetScope = 'resourceGroup'

// Parameters
@description('The name of the Front Door instance.')
param parFrontDoorName string

@description('The name of the parent DNS zone.')
param parParentDnsName string

@description('The name of the resource group containing the DNS zone.')
param parDnsResourceGroupName string

@description('The name of the workload.')
param parWorkloadName string

@description('The hostname of the origin server.')
param parOriginHostName string

@description('The hostname prefix of the DNS zone.')
param parDnsZoneHostnamePrefix string

@description('The hostname of the custom domain.')
param parCustomHostname string

@description('The tags to apply to all resources in this deployment.')
param parTags object

// Existing In-Scope Resources
resource frontDoor 'Microsoft.Cdn/profiles@2021-06-01' existing = {
  name: parFrontDoorName
}

// Existing Out-Of-Scope Resources
resource parentDnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: parParentDnsName
  scope: resourceGroup(parDnsResourceGroupName)
}

// Module Resources
resource frontDoorEndpoint 'Microsoft.Cdn/profiles/afdendpoints@2021-06-01' = {
  parent: frontDoor
  name: parWorkloadName
  location: 'Global'
  tags: parTags

  properties: {
    enabledState: 'Enabled'
  }
}

resource frontDoorOriginGroup 'Microsoft.Cdn/profiles/origingroups@2021-06-01' = {
  parent: frontDoor
  name: '${parWorkloadName}-origin-group'

  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }

    healthProbeSettings: {
      probePath: '/'
      probeRequestType: 'HEAD'
      probeProtocol: 'Http'
      probeIntervalInSeconds: 100
    }

    sessionAffinityState: 'Disabled'
  }
}

resource frontDoorOrigin 'Microsoft.Cdn/profiles/origingroups/origins@2021-06-01' = {
  parent: frontDoorOriginGroup
  name: '${parWorkloadName}-origin'

  properties: {
    hostName: parOriginHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: parOriginHostName
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
}

resource frontDoorCustomDomain 'Microsoft.Cdn/profiles/customdomains@2021-06-01' = {
  parent: frontDoor
  name: '${parWorkloadName}-custom-domain'

  properties: {
    hostName: parCustomHostname
    tlsSettings: {
      certificateType: 'ManagedCertificate'
      minimumTlsVersion: 'TLS12'
    }

    azureDnsZone: {
      id: parentDnsZone.id
    }
  }
}

// TODO: Replace at some point
resource tempCustomDomain 'Microsoft.Cdn/profiles/customdomains@2021-06-01' = {
  parent: frontDoor
  name: '${parWorkloadName}-temp-custom-domain'

  properties: {
    hostName: 'portal.xtremeidiots.com'
    tlsSettings: {
      certificateType: 'ManagedCertificate'
      minimumTlsVersion: 'TLS12'
    }
  }
}

resource frontDoorRoute 'Microsoft.Cdn/profiles/afdendpoints/routes@2021-06-01' = {
  parent: frontDoorEndpoint
  name: '${parWorkloadName}-route'

  properties: {
    customDomains: [
      {
        id: frontDoorCustomDomain.id
      }
      {
        id: tempCustomDomain.id
      }
    ]

    originGroup: {
      id: frontDoorOriginGroup.id
    }

    ruleSets: []
    supportedProtocols: [
      'Https'
    ]
    patternsToMatch: [
      '/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
    enabledState: 'Enabled'
  }
}

module dnsCNAME 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/frontdoorcname:latest' = {
  name: '${deployment().name}-frontdoorcname'
  scope: resourceGroup(parDnsResourceGroupName)

  params: {
    domain: parParentDnsName
    subdomain: parDnsZoneHostnamePrefix
    cname: frontDoorEndpoint.properties.hostName
    cnameValidationToken: frontDoorCustomDomain.properties.validationProperties.validationToken
    tags: parTags
  }
}
