# Quick Start Guide

## Prerequisites

- Docker Desktop installed and running
- (Optional) .NET 8 SDK for local development

## Start Everything with Docker

### Recommended: Staged Startup (for SQL Server issues)

**Windows (PowerShell):**
```powershell
.\start-sqlserver-first.ps1
```

This script starts SQL Server first, waits for it to be ready, then starts other services.

### Alternative: Standard Startup

**Windows (PowerShell):**
```powershell
.\start.ps1
```

**Linux/Mac (Bash):**
```bash
chmod +x start.sh
./start.sh
```

### Manual Start
```bash
# Start SQL Server first and wait
docker-compose up -d sqlserver
# Wait 60-90 seconds, then start others
docker-compose up -d
```

## Verify Services

Check service status:
```bash
docker-compose ps
```

View logs:
```bash
docker-compose logs -f [service-name]
```

## Test the API

### 1. Register a User
```bash
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin123!",
  "firstName": "Admin",
  "lastName": "User"
}
```

### 2. Login
```bash
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

Save the `token` from the response.

### 3. Create a Product (Admin)
```bash
POST http://localhost:5000/api/product
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "stock": 50
}
```

### 4. Get Products
```bash
GET http://localhost:5000/api/product
Authorization: Bearer {your-token}
```

### 5. Create an Order
```bash
POST http://localhost:5000/api/order
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "items": [
    {
      "productId": "{product-id-from-step-3}",
      "quantity": 2,
      "price": 999.99
    }
  ]
}
```

## Stop Services

```bash
docker-compose down
```

To remove volumes (databases):
```bash
docker-compose down -v
```

## Troubleshooting

### Services won't start
1. Check Docker is running: `docker ps`
2. Check ports are available: `netstat -an | findstr "5000 5001 5002 5003"`
3. View logs: `docker-compose logs`

### Database connection errors
- Wait for SQL Server to be fully ready (takes ~30 seconds)
- Check SQL Server logs: `docker-compose logs sqlserver`

### RabbitMQ connection errors
- Check RabbitMQ is running: `docker-compose ps rabbitmq`
- Access management UI: http://localhost:15672

## Next Steps

- Read the full [README.md](README.md) for detailed documentation
- Explore Swagger UI at each service's endpoint
- Check RabbitMQ Management UI for message queues
- Review health check endpoints: `/health` on each service

