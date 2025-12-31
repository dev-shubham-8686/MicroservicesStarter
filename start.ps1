# PowerShell script to start the microservices solution

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Microservices Starter - Startup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "Docker is running" -ForegroundColor Green
} catch {
    Write-Host "Error: Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting infrastructure services first (SQL Server and RabbitMQ)..." -ForegroundColor Yellow
docker-compose up -d sqlserver rabbitmq

Write-Host ""
Write-Host "Waiting for SQL Server to be ready (this may take 60-90 seconds)..." -ForegroundColor Yellow
$maxWait = 120
$waited = 0
$sqlHealthy = $false

while ($waited -lt $maxWait -and -not $sqlHealthy) {
    Start-Sleep -Seconds 5
    $waited += 5
    $health = docker inspect sqlserver --format='{{.State.Health.Status}}' 2>$null
    if ($health -eq "healthy") {
        $sqlHealthy = $true
        Write-Host "SQL Server is healthy!" -ForegroundColor Green
    } else {
        Write-Host "Waiting for SQL Server... ($waited seconds)" -ForegroundColor Yellow
    }
}

if (-not $sqlHealthy) {
    Write-Host "Warning: SQL Server may not be fully ready, but continuing..." -ForegroundColor Yellow
    Write-Host "You can check SQL Server logs with: docker-compose logs sqlserver" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Starting application services..." -ForegroundColor Yellow
docker-compose up -d

Write-Host ""
Write-Host "Waiting for services to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host ""
Write-Host "Service Status:" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Services are starting up!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  API Gateway:     http://localhost:5000" -ForegroundColor White
Write-Host "  Identity Service: http://localhost:5001/swagger" -ForegroundColor White
Write-Host "  Product Service:  http://localhost:5002/swagger" -ForegroundColor White
Write-Host "  Order Service:    http://localhost:5003/swagger" -ForegroundColor White
Write-Host "  RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""
Write-Host "To view logs: docker-compose logs -f [service-name]" -ForegroundColor Yellow
Write-Host "To stop: docker-compose down" -ForegroundColor Yellow
Write-Host ""

