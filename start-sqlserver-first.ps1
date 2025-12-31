# PowerShell script to start services with SQL Server first

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Microservices Starter - Staged Startup" -ForegroundColor Cyan
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
Write-Host "Step 1: Starting SQL Server..." -ForegroundColor Yellow
docker-compose up -d sqlserver

Write-Host ""
Write-Host "Waiting for SQL Server to initialize (60-90 seconds)..." -ForegroundColor Yellow
Write-Host "This is normal - SQL Server takes time to start." -ForegroundColor Gray

$maxWait = 120
$waited = 0
$sqlReady = $false

while ($waited -lt $maxWait -and -not $sqlReady) {
    Start-Sleep -Seconds 5
    $waited += 5
    
    # Check if container is running
    $status = docker ps --filter "name=sqlserver" --format "{{.Status}}" 2>$null
    if ($status -match "Up") {
        # Try to connect
        $result = docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" -C 2>&1
        if ($LASTEXITCODE -eq 0) {
            $sqlReady = $true
            Write-Host "SQL Server is ready!" -ForegroundColor Green
        } else {
            Write-Host "Waiting for SQL Server... ($waited seconds) - Container is running, waiting for database..." -ForegroundColor Yellow
        }
    } else {
        Write-Host "Waiting for SQL Server container... ($waited seconds)" -ForegroundColor Yellow
    }
}

if (-not $sqlReady) {
    Write-Host ""
    Write-Host "Warning: SQL Server may not be fully ready." -ForegroundColor Yellow
    Write-Host "Checking SQL Server logs..." -ForegroundColor Yellow
    docker-compose logs --tail 20 sqlserver
    Write-Host ""
    $continue = Read-Host "Continue anyway? (y/n)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        Write-Host "Aborted." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Step 2: Starting RabbitMQ..." -ForegroundColor Yellow
docker-compose up -d rabbitmq
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "Step 3: Starting application services..." -ForegroundColor Yellow
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
Write-Host "To check SQL Server: docker-compose logs sqlserver" -ForegroundColor Yellow
Write-Host "To stop: docker-compose down" -ForegroundColor Yellow
Write-Host ""

