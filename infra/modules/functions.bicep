param name string
param location string
param keyVaultUri string
param tags object

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: replace('st${name}', '-', '')
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${name}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    RetentionInDays: 90
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-${name}'
  location: location
  tags: tags
  sku: { name: 'Y1', tier: 'Dynamic' }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: 'functionapp'
  identity: { type: 'SystemAssigned' }  // managed identity — no stored credentials
  properties: {
    serverFarmId: hostingPlan.id
    httpsOnly: true
    siteConfig: {
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'KeyVaultUri'
          value: keyVaultUri
        }
        {
          name: 'SqlConnectionStringSecretName'
          value: 'SqlConnectionString'
        }
      ]
    }
  }
}

output defaultHostName string = functionApp.properties.defaultHostName
output identityPrincipalId string = functionApp.identity.principalId
