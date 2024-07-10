@description('The location into which your Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Select the type of environment you want to provision. Allowed values are "production" and "test".')
@allowed([
  'production'
  'test'
])
param environment string

@description('A unique suffix to add to resource names that need to be globally unique.')
@maxLength(13)
param resourceNameSuffix string = uniqueString(resourceGroup().id)

@description('The URL to the product review API.')
param reviewApiUrl string

@secure()
@description('The API key to use when accessing the product review API.')
param reviewApiKey string

// Define the names for resources
var appServiceAppName = 'todo-app-${resourceNameSuffix}'
var appServicePlanName = 'todo-app-${resourceNameSuffix}'
var logAnalyticsWorkspaceName = 'todo-app-${resourceNameSuffix}'
var applicationInsightsName = 'todo-app-${resourceNameSuffix}'
var storageAccountName = 'todoapp${resourceNameSuffix}'

// Define the SKUs for each component based on the environment type.
var environmentConfigurationMap = {
  production: {
    appServicePlan: {
      sku: {
        name: 'F1' // name: 'S1'
        // capacity: 1
      }
    }
    storageAccount: {
      sku: {
        name: 'Standard_GRS' // name: 'Standard_LRS'
      }
    }
  }
  test: {
    appServicePlan: {
      sku: {
        name: 'F1'
      }
    }
    storageAccount: {
      sku: {
        name: 'Standard_GRS'
      }
    }
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: environmentConfigurationMap[environment].appServicePlan.sku
  properties: {
    reserved: true
  }
}

resource appServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceAppName
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'ReviewApiUrl'
          value: reviewApiUrl
        }
        {
          name: 'ReviewApiKey'
          value: reviewApiKey
        }
      ]
    }
  }
}

// https://learn.microsoft.com/en-us/azure/templates/microsoft.keyvault/vaults?pivots=deployment-language-bicep
resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: appServiceAppName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    // accessPolicies: [
    //   {
    //     tenantId: subscription().tenantId
    //     objectId: appServiceApp.identity.principalId
    //     permissions: {
    //       keys: ['get']
    //       secrets: ['get']
    //     }
    //   }
    // ]
  }
}

// https://learn.microsoft.com/en-us/azure/templates/microsoft.authorization/roleassignments?pivots=deployment-language-bicep
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'todo-app-key-vault-${resourceNameSuffix}'
  scope: keyVault
  properties: {
    principalId: appServiceApp.identity.principalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    //'/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.Authorization/roleDefinitions/4633458b-17de-408a-b874-0445c86b69e6'
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    Flow_Type: 'Bluefield'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: environmentConfigurationMap[environment].storageAccount.sku
}

output appServiceAppName string = appServiceApp.name
output appServiceAppHostName string = appServiceApp.properties.defaultHostName
