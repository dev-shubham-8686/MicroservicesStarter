# Microservices Starter - .NET 8

A production-ready microservices solution built with .NET 8, featuring industry-standard practices including API Gateway, JWT Authentication, SQL Server, RabbitMQ messaging, and Docker containerization.

## Architecture

This solution consists of the following microservices:

1. **API Gateway** - Single entry point using Ocelot for routing and authentication
2. **Identity Service** - Handles user authentication and authorization with JWT tokens
3. **Product Service** - Manages product catalog with CRUD operations
4. **Order Service** - Handles order processing with event-driven architecture

## Technology Stack

- **.NET 8** - Latest .NET framework
- **SQL Server** - Database for all services
- **RabbitMQ** - Message broker for event-driven communication
- **Ocelot** - API Gateway
- **JWT** - Authentication and authorization
- **Entity Framework Core** - ORM
- **MassTransit** - Message bus abstraction
- **Built-in Logging** - .NET logging framework
- **Docker** - Containerization
- **Health Checks** - Service health monitoring

## Prerequisites

- Docker Desktop (for Windows/Mac) or Docker Engine (for Linux)
- .NET 8 SDK (for local development)
- Visual Studio 2022 or VS Code (optional)

## 📚 Documentation

- **[Complete Docker & Architecture Guide](DOCKER_GUIDE.md)** - Comprehensive guide covering Docker fundamentals, Dockerfiles, Docker Compose, architecture, configuration, and troubleshooting (Beginner to Advanced)
- **[Docker Quick Reference](DOCKER_QUICK_REFERENCE.md)** - Quick reference card for common Docker commands and configurations
- **[Architecture Overview](ARCHITECTURE.md)** - Detailed system architecture and design patterns
- **[Quick Start Guide](QUICKSTART.md)** - Get up and running quickly
- **[Troubleshooting](TROUBLESHOOTING.md)** - Common issues and solutions

## Quick Start with Docker

1. **Clone the repository** (if applicable) or navigate to the project directory

2. **Start all services using Docker Compose:**
   ```bash
   docker-compose up -d
   ```

3. **Verify services are running:**
   ```bash
   docker-compose ps
   ```

4. **Access the services:**
   - API Gateway: http://localhost:5000
   - Identity Service: http://localhost:5001/swagger
   - Product Service: http://localhost:5002/swagger
   - Order Service: http://localhost:5003/swagger
   - RabbitMQ Management: http://localhost:15672 (guest/guest)
   - SQL Server: localhost:1433 (sa/YourStrong@Passw0rd)

## Local Development

### 1. Start Infrastructure Services

Start SQL Server and RabbitMQ using Docker:
```bash
docker-compose up -d sqlserver rabbitmq
```

### 2. Update Connection Strings

Update `appsettings.Development.json` files in each service to use:
- SQL Server: `localhost,1433`
- RabbitMQ: `localhost`

### 3. Run Services

Run each service individually:

```bash
# Identity Service
cd src/services/IdentityService
dotnet run

# Product Service (in a new terminal)
cd src/services/ProductService
dotnet run

# Order Service (in a new terminal)
cd src/services/OrderService
dotnet run

# API Gateway (in a new terminal)
cd src/gateway/ApiGateway
dotnet run
```

## API Usage

### 1. Register a User

```bash
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### 2. Login

```bash
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

Response includes a JWT token:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresAt": "2024-01-01T00:00:00Z",
  "user": { ... }
}
```

### 3. Create a Product (Admin only)

```bash
POST http://localhost:5000/api/product
Authorization: Bearer {token}
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
Authorization: Bearer {token}
```

### 5. Create an Order

```bash
POST http://localhost:5000/api/order
Authorization: Bearer {token}
Content-Type: application/json

{
  "items": [
    {
      "productId": "{product-id}",
      "quantity": 2,
      "price": 999.99
    }
  ]
}
```

### 6. Get My Orders

```bash
GET http://localhost:5000/api/order/my-orders
Authorization: Bearer {token}
```

## Project Structure

```
MicroservicesStarter/
├── src/
│   ├── gateway/
│   │   └── ApiGateway/          # API Gateway service
│   ├── services/
│   │   ├── IdentityService/      # Authentication & Authorization
│   │   ├── ProductService/       # Product management
│   │   └── OrderService/         # Order processing
│   └── shared/
│       ├── Shared.Contracts/     # Shared DTOs and events
│       └── Shared.Infrastructure/ # Shared infrastructure code
├── docker-compose.yml            # Docker Compose configuration
└── README.md                     # This file
```

## Features

### Authentication & Authorization
- JWT-based authentication
- Role-based authorization (Admin, User)
- Token validation across services
- Secure password hashing with BCrypt

### Event-Driven Architecture
- RabbitMQ for message queuing
- MassTransit for message bus abstraction
- Event publishing and consumption
- Asynchronous communication between services

### Database
- SQL Server for persistent storage
- Entity Framework Core for data access
- Separate database per service
- Automatic database creation on startup

### API Gateway
- Ocelot for routing
- Centralized authentication
- Request aggregation
- Load balancing ready

### Health Checks
- Health check endpoints for all services
- Database connectivity checks
- Service status monitoring

### Logging
- Built-in .NET logging framework
- Console and file logging
- Request/response logging
- Error tracking

## Configuration

### Environment Variables

Key configuration options can be set via environment variables:

- `ConnectionStrings__DefaultConnection` - SQL Server connection string
- `Jwt__Key` - JWT signing key (must be at least 32 characters)
- `Jwt__Issuer` - JWT issuer
- `Jwt__Audience` - JWT audience
- `RabbitMQ__Host` - RabbitMQ host
- `RabbitMQ__Username` - RabbitMQ username
- `RabbitMQ__Password` - RabbitMQ password

### Docker Compose Configuration

The `docker-compose.yml` file includes:
- Service definitions with health checks
- Network configuration
- Volume persistence for databases
- Dependency management

## Security Considerations

1. **Change default passwords** in production
2. **Use strong JWT keys** (at least 32 characters)
3. **Enable HTTPS** in production
4. **Implement rate limiting**
5. **Add API versioning**
6. **Use secrets management** (Azure Key Vault, AWS Secrets Manager, etc.)
7. **Implement CORS policies** appropriately
8. **Add request validation** and sanitization

## Monitoring & Observability

- Health check endpoints: `/health` on each service
- Built-in .NET logging
- RabbitMQ Management UI for message monitoring
- SQL Server monitoring tools

## Development Best Practices

1. **Separate databases** per service (already implemented)
2. **API Gateway** for external communication
3. **Event-driven** communication between services
4. **Health checks** for service monitoring
5. **Structured logging** for debugging
6. **Docker** for consistent environments
7. **Configuration management** via appsettings

## Troubleshooting

### Services not starting
- Check Docker is running: `docker ps`
- Check logs: `docker-compose logs [service-name]`
- Verify ports are not in use

### Database connection issues
- Ensure SQL Server container is healthy: `docker-compose ps`
- Check connection string in appsettings
- Verify SQL Server is accepting connections

### RabbitMQ connection issues
- Check RabbitMQ is running: `docker-compose ps rabbitmq`
- Access management UI: http://localhost:15672
- Verify credentials in configuration

### Authentication failures
- Verify JWT key matches across all services
- Check token expiration
- Ensure token is included in Authorization header

## Next Steps

1. Add unit and integration tests
2. Implement API versioning
3. Add distributed tracing (OpenTelemetry)
4. Implement caching (Redis)
5. Add service discovery (Consul/Eureka)
6. Implement circuit breakers (Polly)
7. Add API documentation (OpenAPI/Swagger aggregation)
8. Implement CI/CD pipelines
9. Add monitoring and alerting (Prometheus/Grafana)
10. Implement rate limiting

## License

This project is provided as-is for educational and development purposes.

## Contributing

Feel free to submit issues and enhancement requests!

