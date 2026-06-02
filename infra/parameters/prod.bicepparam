using '../main.bicep'

param environment = 'prod'
param location = 'centralindia'
param sqlAdminLogin = 'sqladmin'
// sqlAdminPassword: supply via pipeline secret — never commit here
