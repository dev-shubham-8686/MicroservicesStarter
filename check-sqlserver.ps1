# PowerShell script to check SQL Server health

Write-Host "Checking SQL Server container status..." -ForegroundColor Yellow

$containerName = "sqlserver"

# Check if container exists
$container = docker ps -a --filter "name=$containerName" --format "{{.Names}}"
if (-not $container) {
    Write-Host "SQL Server container not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Container found: $container" -ForegroundColor Green

# Check container status
Write-Host "`nContainer Status:" -ForegroundColor Cyan
docker ps --filter "name=$containerName" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Check health status
Write-Host "`nHealth Status:" -ForegroundColor Cyan
$health = docker inspect $containerName --format='{{.State.Health.Status}}'
Write-Host "Health: $health" -ForegroundColor $(if ($health -eq "healthy") { "Green" } else { "Yellow" })

# Show recent logs
Write-Host "`nRecent Logs (last 20 lines):" -ForegroundColor Cyan
docker logs --tail 20 $containerName

# Try to connect
Write-Host "`nTesting SQL Server connection..." -ForegroundColor Yellow
$result = docker exec $containerName /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION" -C 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "Connection successful!" -ForegroundColor Green
    Write-Host $result
} else {
    Write-Host "Connection failed!" -ForegroundColor Red
    Write-Host $result
    Write-Host "`nTroubleshooting steps:" -ForegroundColor Yellow
    Write-Host "1. Wait 60-90 seconds for SQL Server to fully start"
    Write-Host "2. Check Docker Desktop has at least 2GB RAM allocated"
    Write-Host "3. Review full logs: docker logs $containerName"
    Write-Host "4. Try restarting: docker-compose restart sqlserver"
}

