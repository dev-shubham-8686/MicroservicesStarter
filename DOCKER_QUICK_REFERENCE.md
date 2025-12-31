# Docker Quick Reference Card

A quick reference guide for common Docker and Docker Compose commands and configurations.

---

## 🚀 Quick Commands

### Starting Services

```bash
# Start all services in background
docker-compose up -d

# Start specific service
docker-compose up -d sqlserver

# Start and rebuild images
docker-compose up -d --build

# Start without cache (clean build)
docker-compose build --no-cache
docker-compose up -d
```

### Stopping Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (⚠️ deletes data)
docker-compose down -v

# Stop specific service
docker-compose stop identityservice
```

### Viewing Logs

```bash
# View all logs
docker-compose logs

# View specific service logs
docker-compose logs identityservice

# Follow logs (real-time)
docker-compose logs -f identityservice

# Last 100 lines
docker-compose logs --tail=100 identityservice
```

### Service Status

```bash
# List running containers
docker-compose ps

# List all containers (including stopped)
docker-compose ps -a

# View resource usage
docker stats

# Inspect service
docker-compose config
```

### Container Management

```bash
# Execute command in container
docker exec -it identityservice /bin/bash
docker exec -it sqlserver /bin/bash

# View container details
docker inspect identityservice

# Restart service
docker-compose restart identityservice

# Rebuild and restart
docker-compose up -d --build identityservice
```

---

## 🔧 Configuration Quick Reference

### Ports

| Service | Host Port | Container Port | Access URL |
|---------|-----------|----------------|------------|
| API Gateway | 5000 | 80 | http://localhost:5000 |
| Identity Service | 5001 | 80 | http://localhost:5001 |
| Product Service | 5002 | 80 | http://localhost:5002 |
| Order Service | 5003 | 80 | http://localhost:5003 |
| SQL Server | 1433 | 1433 | localhost:1433 |
| RabbitMQ AMQP | 5672 | 5672 | localhost:5672 |
| RabbitMQ UI | 15672 | 15672 | http://localhost:15672 |

### Service Names (Internal Communication)

| Service | Internal Hostname | Usage |
|---------|-------------------|-------|
| SQL Server | `sqlserver` | Connection string: `Server=sqlserver` |
| RabbitMQ | `rabbitmq` | Connection: `RabbitMQ__Host=rabbitmq` |
| Identity Service | `identityservice` | Internal routing |
| Product Service | `productservice` | Internal routing |
| Order Service | `orderservice` | Internal routing |
| API Gateway | `apigateway` | Internal routing |

### Default Credentials

| Service | Username | Password | ⚠️ Change in Production |
|---------|----------|----------|------------------------|
| SQL Server | `sa` | `YourStrong@Passw0rd` | ✅ Yes |
| RabbitMQ | `guest` | `guest` | ✅ Yes |
| JWT Key | N/A | `YourSuperSecretKeyThatIsAtLeast32CharactersLong!` | ✅ Yes |

### Environment Variables

#### Connection String Format
```
Server=sqlserver;Database=IdentityDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Connect Timeout=30;
```

#### JWT Configuration
```
Jwt__Key=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
Jwt__Issuer=MicroservicesStarter
Jwt__Audience=MicroservicesStarter
```

#### RabbitMQ Configuration
```
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest
```

---

## 🗄️ Database Quick Reference

### SQL Server Connection

**From Host Machine:**
```
Server=localhost,1433;Database=IdentityDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

**From Container (Internal):**
```
Server=sqlserver;Database=IdentityDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

### Databases

| Database | Service | Purpose |
|----------|---------|---------|
| IdentityDb | Identity Service | Users, Roles, UserRoles |
| ProductDb | Product Service | Products catalog |
| OrderDb | Order Service | Orders, OrderItems |

---

## 🔍 Troubleshooting Commands

### Check Port Availability

**Windows:**
```powershell
netstat -ano | findstr :5000
```

**Linux/Mac:**
```bash
lsof -i :5000
```

### Check Service Health

```bash
# Check if service is running
docker-compose ps

# Check service logs
docker-compose logs sqlserver

# Test connectivity from container
docker exec -it identityservice ping sqlserver
```

### Clean Up

```bash
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove unused volumes (⚠️ careful!)
docker volume prune

# Remove everything unused
docker system prune -a
```

### Network Diagnostics

```bash
# List networks
docker network ls

# Inspect network
docker network inspect microservicesstarter_microservices-network

# Test DNS resolution
docker exec -it identityservice nslookup sqlserver
```

---

## 📝 Dockerfile Commands

### Build Image

```bash
# Build from Dockerfile
docker build -t myapp:latest -f src/services/IdentityService/Dockerfile .

# Build without cache
docker build --no-cache -t myapp:latest -f src/services/IdentityService/Dockerfile .
```

### Run Container

```bash
# Run container
docker run -d -p 5001:80 --name identityservice identityservice:latest

# Run with environment variables
docker run -d -p 5001:80 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Server=sqlserver;..." \
  --name identityservice identityservice:latest
```

---

## 🔐 Security Checklist

- [ ] Change SQL Server password
- [ ] Change RabbitMQ credentials
- [ ] Use strong JWT key (32+ characters)
- [ ] Don't commit .env files
- [ ] Use secrets management in production
- [ ] Enable HTTPS in production
- [ ] Set resource limits
- [ ] Review exposed ports
- [ ] Use specific image tags (not `latest`)
- [ ] Regular security updates

---

## 📊 Monitoring Commands

### Resource Usage

```bash
# Real-time stats
docker stats

# Container resource usage
docker stats identityservice productservice orderservice

# Disk usage
docker system df
```

### Health Checks

```bash
# Check health status
docker inspect --format='{{.State.Health.Status}}' identityservice

# View health check logs
docker inspect identityservice | grep -A 10 Health
```

---

## 🛠️ Development Workflow

### 1. Start Infrastructure

```bash
docker-compose up -d sqlserver rabbitmq
```

### 2. Wait for Services

```bash
# Wait for SQL Server (takes 30-90 seconds)
docker-compose logs -f sqlserver

# Wait for RabbitMQ
docker-compose logs -f rabbitmq
```

### 3. Start Application Services

```bash
docker-compose up -d identityservice productservice orderservice apigateway
```

### 4. Verify All Services

```bash
docker-compose ps
```

### 5. View Logs

```bash
docker-compose logs -f
```

---

## 🔄 Common Workflows

### Rebuild After Code Changes

```bash
# Rebuild specific service
docker-compose build identityservice
docker-compose up -d identityservice

# Rebuild all services
docker-compose build
docker-compose up -d
```

### Reset Everything

```bash
# Stop and remove everything
docker-compose down -v

# Clean build
docker-compose build --no-cache

# Start fresh
docker-compose up -d
```

### Update Single Service

```bash
# Stop service
docker-compose stop identityservice

# Remove container
docker-compose rm -f identityservice

# Rebuild
docker-compose build identityservice

# Start
docker-compose up -d identityservice
```

---

## 📚 Additional Resources

- **Full Documentation:** [DOCKER_GUIDE.md](DOCKER_GUIDE.md)
- **Architecture:** [ARCHITECTURE.md](ARCHITECTURE.md)
- **Troubleshooting:** [TROUBLESHOOTING.md](TROUBLESHOOTING.md)

---

## 💡 Pro Tips

1. **Use service names** for inter-container communication (not `localhost`)
2. **Check logs first** when troubleshooting
3. **Wait for SQL Server** to fully start (30-90 seconds)
4. **Use health checks** for production
5. **Keep images updated** with security patches
6. **Use .env files** for environment-specific config
7. **Don't commit secrets** to version control
8. **Test locally** before deploying
9. **Monitor resource usage** regularly
10. **Document custom configurations**

---

**Last Updated:** 2024

