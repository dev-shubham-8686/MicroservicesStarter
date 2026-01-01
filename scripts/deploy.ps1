# Deployment Script for Microservices (PowerShell)
# Usage: .\scripts\deploy.ps1 [environment] [version]

param(
    [string]$Environment = "staging",
    [string]$Version = "latest",
    [string]$Registry = "ghcr.io"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Deploying to $Environment environment" -ForegroundColor Green
Write-Host "Version: $Version"
Write-Host "Registry: $Registry"

# Load environment variables
$envFile = ".env.$Environment"
if (Test-Path $envFile) {
    Write-Host "Loading $envFile" -ForegroundColor Yellow
    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^([^#][^=]+)=(.*)$') {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
        }
    }
} else {
    Write-Host "⚠️  Warning: $envFile not found" -ForegroundColor Yellow
}

# Set version
$env:VERSION = $Version
$env:REGISTRY = $Registry

# Pull latest images
Write-Host "📥 Pulling Docker images..." -ForegroundColor Cyan
docker-compose -f docker-compose.prod.yml pull

# Stop existing containers
Write-Host "🛑 Stopping existing containers..." -ForegroundColor Yellow
docker-compose -f docker-compose.prod.yml down

# Start services
Write-Host "▶️  Starting services..." -ForegroundColor Green
docker-compose -f docker-compose.prod.yml up -d

# Wait for services to be healthy
Write-Host "⏳ Waiting for services to be healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Health checks
Write-Host "🏥 Running health checks..." -ForegroundColor Cyan

function Check-Health {
    param(
        [string]$Service,
        [int]$Port,
        [int]$MaxAttempts = 30
    )
    
    $attempt = 1
    while ($attempt -le $MaxAttempts) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:$Port/health" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ $Service is healthy" -ForegroundColor Green
                return $true
            }
        } catch {
            # Service not ready yet
        }
        
        Write-Host "⏳ Waiting for $Service... ($attempt/$MaxAttempts)" -ForegroundColor Yellow
        Start-Sleep -Seconds 2
        $attempt++
    }
    
    Write-Host "❌ $Service failed health check" -ForegroundColor Red
    return $false
}

$apiGatewayPort = if ($env:API_GATEWAY_PORT) { [int]$env:API_GATEWAY_PORT } else { 5000 }
$identityPort = if ($env:IDENTITY_SERVICE_PORT) { [int]$env:IDENTITY_SERVICE_PORT } else { 5001 }
$productPort = if ($env:PRODUCT_SERVICE_PORT) { [int]$env:PRODUCT_SERVICE_PORT } else { 5002 }
$orderPort = if ($env:ORDER_SERVICE_PORT) { [int]$env:ORDER_SERVICE_PORT } else { 5003 }

$allHealthy = $true
$allHealthy = $allHealthy -and (Check-Health "API Gateway" $apiGatewayPort)
$allHealthy = $allHealthy -and (Check-Health "Identity Service" $identityPort)
$allHealthy = $allHealthy -and (Check-Health "Product Service" $productPort)
$allHealthy = $allHealthy -and (Check-Health "Order Service" $orderPort)

if (-not $allHealthy) {
    Write-Host "❌ Deployment failed - some services are not healthy" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Services are running:"
Write-Host "  - API Gateway: http://localhost:$apiGatewayPort"
Write-Host "  - Identity Service: http://localhost:$identityPort"
Write-Host "  - Product Service: http://localhost:$productPort"
Write-Host "  - Order Service: http://localhost:$orderPort"
$rabbitMqPort = if ($env:RABBITMQ_MGMT_PORT) { $env:RABBITMQ_MGMT_PORT } else { 15672 }
Write-Host "  - RabbitMQ Management: http://localhost:$rabbitMqPort"

