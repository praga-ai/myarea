param serverName string
param databaseName string
param location string
param adminLogin string
@secure()
param adminPassword string
param tags object

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    administratorLogin: adminLogin
    administratorLoginPassword: adminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'  // lock down to private endpoint in prod
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// Allow Azure services (Functions) to reach SQL
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  tags: tags
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: 60      // serverless — pauses after 1 h idle (dev cost saving)
    minCapacity: '0.5'
    zoneRedundant: false
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
output serverId string = sqlServer.id
