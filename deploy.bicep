@description('Azure region for all resources')
param location string = resourceGroup().location
@description('Base name for all resources; will be uniquified')
@minLength(3)
param baseName string

// Generate a unique suffix for globally named resources
var uniqueSuffix = uniqueString(resourceGroup().id)

// 1. Storage Account (global name must be unique)
resource sa 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: toLower('${baseName}sa${uniqueSuffix}')
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
}

// 2. App Insights
resource ai 'Microsoft.Insights/components@2020-02-02' = {
  name: '${baseName}-ai'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

// 3. Consumption Plan - Updated to match the ARM template
resource plan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: '${baseName}-plan'
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
}

// 4. Communication Services (Email) – must use 'global' location
resource comm 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: '${baseName}-comm'
  location: 'global'
  properties: {
    dataLocation: 'Europe'
  }
}

// 5. Blob Services (needed for container)
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2024-01-01' = {
  parent: sa
  name: 'default'
}

// 6. Function App - without linuxFxVersion initially
var storageKey = sa.listKeys().keys[0].value
var storageConnString = 'DefaultEndpointsProtocol=https;AccountName=${sa.name};AccountKey=${storageKey};EndpointSuffix=${environment().suffixes.storage}'
resource func 'Microsoft.Web/sites@2024-11-01' = {
  name: '${baseName}-func'
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: plan.id
    reserved: true
    siteConfig: { 
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'
      appSettings: [
        { 
            name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
            value: ai.properties.ConnectionString 
        }
        { 
            name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
            value: ai.properties.InstrumentationKey 
        }
        {
          name: 'AzureWebJobsStorage'
          value: storageConnString
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: '0'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'DOTNET-ISOLATED'
        }
        {
          name: 'AcsEmailConnectionString'
          value: comm.listKeys().primaryConnectionString
        }
        {
          name: 'BuySignalNotifier__SenderEmailAddress'
          value: 'your-email@example.com'
        }
      ]
    }
  }
}

resource connStrings 'Microsoft.Web/sites/config@2024-11-01' = {
  parent: func
  name: 'connectionstrings'
  properties: {
    AzureWebJobsStorage: {
      value: storageConnString
      type: 'Custom'
    }
    AcsEmailConnectionString: {
      value: comm.listKeys().primaryConnectionString
      type: 'Custom'
    }
  }
}

// 7. Blob container for your JSON
resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2024-01-01' = {
  parent: blobServices
  name: 'watchlists'
  properties: { 
    publicAccess: 'None' 
  }
}

output functionEndpoint string = func.properties.defaultHostName