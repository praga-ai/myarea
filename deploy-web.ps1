# Survey Admin Web App Deployment Script
# Deploys the React web app to Azure App Service

param (
    [Parameter(Mandatory = $false)]
    [string]$ResourceGroup = "survey-admin-rg",

    [Parameter(Mandatory = $false)]
    [string]$AppServiceName = "app-survey-admin-cs-dev",

    [Parameter(Mandatory = $false)]
    [string]$Location = "centralindia",

    [Parameter(Mandatory = $false)]
    [ValidateSet("build-only", "deploy-only", "build-and-deploy")]
    [string]$Mode = "build-and-deploy"
)

# Color output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error-Custom { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }

Write-Info "========================================"
Write-Info "Survey Admin Web App Deployment Script"
Write-Info "========================================"
Write-Info ""

# Step 1: Build React App
if ($Mode -in "build-only", "build-and-deploy") {
    Write-Info "Step 1: Building React application..."

    try {
        Push-Location -Path "./web"

        # Install dependencies
        Write-Info "Installing npm dependencies..."
        npm install

        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Failed to install dependencies"
            Pop-Location
            exit 1
        }

        # Build production bundle
        Write-Info "Building production bundle..."
        npm run build

        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Failed to build application"
            Pop-Location
            exit 1
        }

        Write-Success "✓ Build completed successfully!"

        # Verify build output
        if (Test-Path "./build") {
            $buildSize = (Get-ChildItem -Path "./build" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
            Write-Info "Build size: $buildSize MB"
        }
        else {
            Write-Error-Custom "Build folder not found!"
            Pop-Location
            exit 1
        }

        Pop-Location
    }
    catch {
        Write-Error-Custom "Error during build: $_"
        Pop-Location
        exit 1
    }

    Write-Info ""
}

# Step 2: Deploy to Azure
if ($Mode -in "deploy-only", "build-and-deploy") {
    Write-Info "Step 2: Deploying to Azure App Service..."

    try {
        # Check if logged in to Azure
        $azureAccount = az account show 2>$null
        if (-not $azureAccount) {
            Write-Info "Not logged in to Azure. Please login..."
            az login
        }

        # Check if resource group exists
        Write-Info "Checking resource group '$ResourceGroup'..."
        $rg = az group exists -n $ResourceGroup

        if ($rg -eq "false") {
            Write-Info "Creating resource group..."
            az group create -n $ResourceGroup -l $Location
            Write-Success "✓ Resource group created!"
        }
        else {
            Write-Info "✓ Resource group exists"
        }

        # Check if App Service exists
        Write-Info "Checking App Service '$AppServiceName'..."
        $appService = az webapp list -g $ResourceGroup --query "[?name=='$AppServiceName'].id" -o tsv

        if (-not $appService) {
            Write-Info "Creating App Service using Bicep..."
            az deployment group create `
                -g $ResourceGroup `
                -f "./bicep/web-app.bicep" `
                -p "./bicep/web-app.parameters.json"

            Write-Success "✓ App Service created!"
        }
        else {
            Write-Info "✓ App Service exists"
        }

        # Create deployment package
        Write-Info "Creating deployment package..."
        Push-Location -Path "./web"

        # Compress build folder
        if (Test-Path "./build") {
            Compress-Archive -Path "./build/*" -DestinationPath "../web-deploy.zip" -Force
            Write-Success "✓ Deployment package created!"
        }

        Pop-Location

        # Deploy to App Service
        Write-Info "Deploying to App Service..."
        az webapp deployment source config-zip `
            -g $ResourceGroup `
            -n $AppServiceName `
            --src "./web-deploy.zip"

        Write-Success "✓ Deployment completed!"

        # Get App Service URL
        $appUrl = az webapp show -g $ResourceGroup -n $AppServiceName --query "defaultHostName" -o tsv
        Write-Success ""
        Write-Success "========================================"
        Write-Success "Deployment Successful!"
        Write-Success "========================================"
        Write-Success "App URL: https://$appUrl"
        Write-Success ""
        Write-Success "Demo Credentials:"
        Write-Success "  Admin: admin@survey.com / Admin@123"
        Write-Success "  Surveyor: surveyor@survey.com / Surveyor@123"
        Write-Success "========================================"

        # Cleanup deployment package
        if (Test-Path "./web-deploy.zip") {
            Remove-Item "./web-deploy.zip" -Force
            Write-Info "Cleanup: Removed deployment package"
        }
    }
    catch {
        Write-Error-Custom "Error during deployment: $_"
        exit 1
    }
}

Write-Info ""
Write-Success "Script execution completed!"
