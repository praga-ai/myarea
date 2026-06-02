using '../main.bicep'

param environment = 'dev'
param location = 'centralindia'
param sqlAdminLogin = 'sqladmin'
// sqlAdminPassword: supply via pipeline secret — never commit here
