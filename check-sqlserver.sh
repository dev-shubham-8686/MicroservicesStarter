#!/bin/bash

# Bash script to check SQL Server health

echo "Checking SQL Server container status..."

CONTAINER_NAME="sqlserver"

# Check if container exists
if ! docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "SQL Server container not found!"
    exit 1
fi

echo "Container found: $CONTAINER_NAME"
echo ""

# Check container status
echo "Container Status:"
docker ps --filter "name=$CONTAINER_NAME" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

# Check health status
echo "Health Status:"
HEALTH=$(docker inspect $CONTAINER_NAME --format='{{.State.Health.Status}}' 2>/dev/null)
if [ "$HEALTH" = "healthy" ]; then
    echo "Health: $HEALTH" | grep --color=always "healthy"
else
    echo "Health: $HEALTH"
fi
echo ""

# Show recent logs
echo "Recent Logs (last 20 lines):"
docker logs --tail 20 $CONTAINER_NAME
echo ""

# Try to connect
echo "Testing SQL Server connection..."
if docker exec $CONTAINER_NAME /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION" -C > /dev/null 2>&1; then
    echo "Connection successful!"
    docker exec $CONTAINER_NAME /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION" -C
else
    echo "Connection failed!"
    echo ""
    echo "Troubleshooting steps:"
    echo "1. Wait 60-90 seconds for SQL Server to fully start"
    echo "2. Check Docker Desktop has at least 2GB RAM allocated"
    echo "3. Review full logs: docker logs $CONTAINER_NAME"
    echo "4. Try restarting: docker-compose restart sqlserver"
fi

