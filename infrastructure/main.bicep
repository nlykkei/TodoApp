@description('The Azure region into which the resources should be deployed.')
param location string = resourceGroup().location

@description('The type of environment. This must be dev or prod.')
@allowed([
  'dev'
  'prod'
])
param environmentType string

@description('A unique suffix to add to resource names that need to be globally unique.')
@maxLength(13)
param resourceNameSuffix string = uniqueString(resourceGroup().id)

var appServiceAppName = 'todo-website-${resourceNameSuffix}'
var appServicePlanName = 'todo-website-plan'
var appStorageAccountName = 'todoweb${resourceNameSuffix}'

// Define the SKUs for each component based on the environment type.
var environmentConfigurationMap = {
  qa: {
    appServicePlan: {
      sku: {
        name: 'F1'
        capacity: 1
      }
    }
    appStorageAccount: {
      sku: {
        name: 'Standard_LRS'
      }
    }
  }
  prod: {
    appServicePlan: {
      sku: {
        name: 'S1'
        capacity: 2
      }
    }
    appStorageAccount: {
      sku: {
        name: 'Standard_ZRS'
      }
    }
  }
}

var appStorageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${appStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${appStorageAccount.listKeys().keys[0].value}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: environmentConfigurationMap[environmentType].appServicePlan.sku
}

resource appServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'appStorageAccountConnectionString'
          value: appStorageAccountConnectionString
        }
      ]
    }
  }
}

resource appStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: appStorageAccountName
  location: location
  kind: 'StorageV2'
  sku: environmentConfigurationMap[environmentType].appStorageAccount.sku
}
