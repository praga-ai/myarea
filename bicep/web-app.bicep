param location string = resourceGroup().location
param appName string = 'survey-admin'
param environment string = 'dev'
param uniqueSuffix string = 'cs'
param tier string = 'Standard'
param skuName string = 'S1'

// Naming variables
var appServicePlanName = 'asp-${appName}-${uniqueSuffix}-${environment}'
var appServiceName = 'app-${appName}-${uniqueSuffix}-${environment}'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: skuName
    tier: tier
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
  tags: {
    environment: environment
    application: appName
    createdDate: utcNow('yyyy-MM-dd')
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'NODE|18-lts'
      minTlsVersion: '1.2'
      http20Enabled: true
      alwaysOn: true
      appCommandLine: ''
      numberOfWorkers: 1
      defaultDocuments: [
        'index.html'
      ]
      appSettings: [
        {
          name: 'REACT_APP_API_BASE_URL'
          value: 'https://func-mobileapp-cs-in.azurewebsites.net/api'
        }
        {
          name: 'REACT_APP_APP_NAME'
          value: 'Survey Admin'
        }
        {
          name: 'REACT_APP_VERSION'
          value: '1.0.0'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'true'
        }
        {
          name: 'ENABLE_ORYX_BUILD'
          value: 'true'
        }
      ]
      virtualApplications: [
        {
          virtualPath: '/'
          physicalPath: 'site\\wwwroot'
          preloadEnabled: true
        }
      ]
    }
    clientAffinityEnabled: false
  }
  tags: {
    environment: environment
    application: appName
    createdDate: utcNow('yyyy-MM-dd')
  }
}

// Web Config for SPA routing
resource webConfig 'Microsoft.Web/sites/config@2022-09-01' = {
  parent: appService
  name: 'web'
  properties: {
    numberOfWorkers: 1
    defaultDocuments: [
      'index.html'
    ]
    netFrameworkVersion: 'v4.0'
    requestTracingEnabled: false
    remoteDebuggingEnabled: false
    httpLoggingEnabled: false
    detailedErrorLoggingEnabled: false
    publishingUsername: 'publish-${appServiceName}'
    scmType: 'None'
    use32BitWorkerProcess: true
    webSocketsEnabled: false
    managedPipelineMode: 'Integrated'
    virtualApplications: [
      {
        virtualPath: '/'
        physicalPath: 'site\\wwwroot'
        preloadEnabled: true
      }
    ]
    loadBalancing: 'LeastRequests'
    experiments: {
      rampUpRules: []
    }
    autoHealEnabled: false
    localMySqlEnabled: false
    ipSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 1
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 1
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictionsUseMain: false
    http20Enabled: true
    minTlsVersion: '1.2'
    scmMinTlsVersion: '1.0'
    ftpsState: 'FtpsOnly'
    preWarmedInstanceCount: 0
    functionAppScaleLimit: 0
    healthCheckPath: ''
    fileChangeAuditEnabled: false
    functionsRuntimeScaleMonitoringEnabled: false
    websiteTimeZone: 'UTC'
    minimumElasticInstanceCount: 0
    azureStorageAccounts: {}
    metadata: [
      {
        name: 'CURRENT_STACK'
        value: 'node'
      }
    ]
    cors: {
      allowedOrigins: [
        '*'
      ]
      supportCredentials: false
    }
    localCacheEnabled: false
    ipSecurityRestrictionsDefaultAction: 'Allow'
    scmIpSecurityRestrictionsDefaultAction: 'Allow'
    vnetRouteAllEnabled: false
    vnetPrivatePortsEnabled: false
    publicNetworkAccessEnabled: true
    numberOfWorkers: 1
  }
}

// Deployment Slot for staging
resource stagingSlot 'Microsoft.Web/sites/slots@2022-09-01' = {
  parent: appService
  name: 'staging'
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'NODE|18-lts'
      minTlsVersion: '1.2'
    }
  }
  tags: {
    environment: 'staging'
    application: appName
    createdDate: utcNow('yyyy-MM-dd')
  }
}

// Diagnostic Settings
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: appService
  name: 'diagnostics-${appServiceName}'
  properties: {
    workspaceId: ''
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// Outputs
output appServiceName string = appService.name
output appServiceId string = appService.id
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output appServicePlanName string = appServicePlan.name
output appServicePlanId string = appServicePlan.id
output deploymentTime string = utcNow('u')
