. "./scripts/createAppRegistrationCredential.ps1" `
    -keyVaultName 'kv-portal-prd-uksouth-01' `
    -applicationName 'portal-events-api-prd' `
    -secretName 'portal-events-api-prd-clientsecret' `
    -secretIdentifier 'fnportalevents'

. "./scripts/createAppRegistrationCredential.ps1" `
    -keyVaultName 'kv-portal-prd-uksouth-01' `
    -applicationName 'portal-repository-api-prd' `
    -secretName 'portal-repository-api-prd-clientsecret' `
    -secretIdentifier 'webportalrepo'