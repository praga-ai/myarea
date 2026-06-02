@description('Environment name (dev, staging, prod)')
param environment string

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('SQL administrator login name')
param sqlAdminLogin string

@description('SQL administrator password')
@secure()
param sqlAdminPassword string

var prefix = 'mobileapp-${environment}'
var tags = { environment: environment, project: 'MobileApp' }

module keyvault 'modules/keyvault.bicep' = {
  name: 'keyvault'
  params: {
    name: 'kv-${prefix}'
    location: location
    tags: tags
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    serverName: 'sql-${prefix}'
    databaseName: 'db-mobileapp'
    location: location
    adminLogin: sqlAdminLogin
    adminPassword: sqlAdminPassword
    tags: tags
  }
}

module functions 'modules/functions.bicep' = {
  name: 'functions'
  params: {
    name: 'func-${prefix}'
    location: location
    keyVaultUri: keyvault.outputs.uri
    tags: tags
  }
}

// Grant the Function App's managed identity access to Key Vault
module kvAccess 'modules/keyvault-access.bicep' = {
  name: 'kvAccess'
  params: {
    keyVaultName: keyvault.outputs.name
    principalId: functions.outputs.identityPrincipalId
  }
}

// Store the SQL connection string in Key Vault
module sqlSecret 'modules/keyvault-secret.bicep' = {
  name: 'sqlSecret'
  params: {
    keyVaultName: keyvault.outputs.name
    secretName: 'SqlConnectionString'
    secretValue: 'Server=tcp:${sql.outputs.fullyQualifiedDomainName},1433;Database=db-mobileapp;Authentication=Active Directory Managed Identity;Encrypt=True;'
  }
  dependsOn: [kvAccess]
}

output functionAppUrl string = functions.outputs.defaultHostName
