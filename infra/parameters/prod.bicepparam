using '../main.bicep'

param environment = 'prod'
param location = 'centralindia'
param sqlAdminLogin = 'sqladmin'
param sqlAdminPassword = 'Placeholder123!' // Will be overridden by --parameters from pipeline
