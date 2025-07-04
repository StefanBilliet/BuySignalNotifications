@description('Azure region for all resources')
param location string = resourceGroup().location
@description('Base name for all resources; will be uniquified')
@minLength(3)
param baseName string

// Generate a unique suffix for globally named resources
var uniqueSuffix = uniqueString(resourceGroup().id)

// 1. Storage Account (global name must be unique)
resource sa 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: toLower('${baseName}sa${uniqueSuffix}')
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
}


// 2. Consumption Plan
resource plan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: '${baseName}-plan'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp'
}

// 3. Communication Services (Email) – must use 'global' location
resource comm 'Microsoft.Communication/communicationServices@2023-03-31' = {
  name: '${baseName}-comm'
  location: 'global'
  properties: {
    dataLocation: 'Europe'
  }
}

// 4. Blob Services (needed for container)
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  parent: sa
  name: 'default'
}

// 6. Function App
resource func 'Microsoft.Web/sites@2021-02-01' = {
  name: '${baseName}-func'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${sa.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${sa.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'AcsEmailConnectionString'
          value: comm.listKeys().primaryConnectionString
        }
        {
          name: 'BuySignalNotifier:SenderEmailAddress'
          value: 'your-email@example.com'
        }
      ]
    }
  }
}

// 7. Blob container for your JSON
resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  parent: blobServices
  name: 'config'
  properties: { 
    publicAccess: 'None' 
  }
}

output functionEndpoint string = func.properties.defaultHostName