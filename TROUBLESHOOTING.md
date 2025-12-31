# Troubleshooting Guide

## SQL Server Container Unhealthy

If you're seeing "dependency failed to start: container sqlserver is unhealthy", follow these steps:

### Solution 1: Wait and Retry

SQL Server can take 60-90 seconds to fully start. Try:

```bash
# Stop all containers
docker-compose down

# Start SQL Server first and wait
docker-compose up -d sqlserver

# Wait 60-90 seconds, then check status
docker-compose ps sqlserver

# Once healthy, start other services
docker-compose up -d
```

### Solution 2: Check SQL Server Logs

```bash
docker-compose logs sqlserver
```

Look for errors or messages indicating why SQL Server isn't starting properly.

### Solution 3: Remove Volumes and Restart

If SQL Server data is corrupted:

```bash
# Stop all containers
docker-compose down

# Remove volumes (WARNING: This deletes all data)
docker-compose down -v

# Start fresh
docker-compose up -d
```

### Solution 4: Manual SQL Server Health Check

Test SQL Server manually:

```bash
# Enter SQL Server container
docker exec -it sqlserver bash

# Run health check command
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" -C
```

### Solution 5: Increase Resources

SQL Server requires sufficient resources. Ensure Docker has:
- At least 2GB RAM allocated
- At least 2 CPU cores

Check Docker Desktop settings → Resources

### Solution 6: Check Port Conflicts

Ensure port 1433 is not in use:

**Windows:**
```powershell
netstat -an | findstr "1433"
```

**Linux/Mac:**
```bash
lsof -i :1433
```

If port is in use, either:
- Stop the conflicting service
- Change SQL Server port in docker-compose.yml

### Solution 7: Use Alternative Health Check

If the health check continues to fail, you can temporarily disable it:

1. Comment out the `depends_on` condition:
```yaml
depends_on:
  # sqlserver:
  #   condition: service_healthy
```

2. Add a startup delay in your application code instead

### Common Issues

#### Issue: Password Complexity
SQL Server requires strong passwords. Ensure your password:
- Has at least 8 characters
- Contains uppercase, lowercase, numbers, and special characters
- Current password: `YourStrong@Passw0rd` meets requirements

#### Issue: EULA Not Accepted
Ensure `ACCEPT_EULA=Y` is set in environment variables.

#### Issue: Insufficient Memory
SQL Server needs at least 2GB RAM. Check Docker Desktop → Settings → Resources.

#### Issue: Slow Startup on Windows
On Windows, SQL Server containers can be slower. The `start_period: 60s` gives it time before health checks begin.

### Verify SQL Server is Running

```bash
# Check container status
docker ps | grep sqlserver

# Check health status
docker inspect sqlserver | grep -A 10 Health

# Test connection from host
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION"
```

### Still Having Issues?

1. Check Docker Desktop is running and has sufficient resources
2. Review full logs: `docker-compose logs`
3. Try starting services one at a time:
   ```bash
   docker-compose up -d sqlserver
   # Wait 90 seconds
   docker-compose up -d rabbitmq
   # Wait 30 seconds
   docker-compose up -d identityservice
   docker-compose up -d productservice
   docker-compose up -d orderservice
   docker-compose up -d apigateway
   ```

