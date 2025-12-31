# Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Client Applications                    │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    API Gateway (Ocelot)                     │
│                    Port: 5000                               │
│  - Routing                                                 │
│  - Authentication                                          │
│  - Request Aggregation                                      │
└─────┬──────────────┬──────────────┬────────────────────────┘
      │              │              │
      ▼              ▼              ▼
┌──────────┐  ┌──────────┐  ┌──────────┐
│Identity  │  │ Product  │  │  Order   │
│ Service  │  │ Service  │  │ Service  │
│ Port:    │  │ Port:    │  │ Port:    │
│  5001    │  │  5002    │  │  5003    │
└────┬─────┘  └────┬─────┘  └────┬─────┘
     │             │              │
     │             │              │
     ▼             ▼              ▼
┌─────────────────────────────────────────┐
│         SQL Server (Port: 1433)         │
│  - IdentityDb                            │
│  - ProductDb                             │
│  - OrderDb                               │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│      RabbitMQ (Ports: 5672, 15672)     │
│  - Message Queue                        │
│  - Event Bus                            │
│  - Service Communication                │
└─────────────────────────────────────────┘
```

## Service Responsibilities

### API Gateway
- **Purpose**: Single entry point for all client requests
- **Technology**: Ocelot
- **Features**:
  - Route requests to appropriate microservices
  - Handle authentication/authorization
  - Aggregate responses
  - Load balancing ready

### Identity Service
- **Purpose**: Authentication and authorization
- **Database**: IdentityDb
- **Features**:
  - User registration and login
  - JWT token generation
  - Role-based access control (Admin, User)
  - Password hashing with BCrypt
  - Token validation

### Product Service
- **Purpose**: Product catalog management
- **Database**: ProductDb
- **Features**:
  - CRUD operations for products
  - Stock management
  - Publishes ProductCreatedEvent to RabbitMQ
  - Admin-only write operations
  - Public read operations (authenticated)

### Order Service
- **Purpose**: Order processing
- **Database**: OrderDb
- **Features**:
  - Order creation and management
  - Consumes ProductCreatedEvent from RabbitMQ
  - Publishes OrderCreatedEvent to RabbitMQ
  - User-specific order retrieval
  - Order status management

## Communication Patterns

### Synchronous Communication
- **HTTP/REST**: Used for direct service-to-service calls through API Gateway
- **JWT Tokens**: Passed in Authorization header for authentication

### Asynchronous Communication
- **RabbitMQ**: Used for event-driven communication
- **MassTransit**: Abstraction layer for message bus
- **Events**: 
  - ProductCreatedEvent
  - OrderCreatedEvent

## Data Flow Examples

### User Registration Flow
```
Client → API Gateway → Identity Service → SQL Server (IdentityDb)
                                    ↓
                              JWT Token
                                    ↓
                              Client
```

### Product Creation Flow
```
Client → API Gateway → Product Service → SQL Server (ProductDb)
                                    ↓
                              ProductCreatedEvent
                                    ↓
                              RabbitMQ
                                    ↓
                              Order Service (Consumer)
```

### Order Creation Flow
```
Client → API Gateway → Order Service → SQL Server (OrderDb)
                                    ↓
                              OrderCreatedEvent
                                    ↓
                              RabbitMQ
```

## Security Architecture

### Authentication Flow
1. User registers/logs in through Identity Service
2. Identity Service validates credentials
3. JWT token is generated and returned
4. Client includes token in subsequent requests
5. API Gateway validates token before routing
6. Each service validates token independently

### Authorization
- **Role-based**: Admin and User roles
- **Claims**: User ID, Email, Roles included in JWT
- **Policy-based**: Services enforce role requirements

## Database Strategy

### Database per Service
- Each microservice has its own database
- Prevents tight coupling
- Allows independent scaling
- Enables technology diversity

### Databases
- **IdentityDb**: User accounts, roles, user-roles
- **ProductDb**: Products catalog
- **OrderDb**: Orders and order items

## Deployment Architecture

### Docker Containers
- Each service runs in its own container
- SQL Server and RabbitMQ run in separate containers
- All containers communicate via Docker network
- Health checks ensure service availability

### Ports
- API Gateway: 5000
- Identity Service: 5001
- Product Service: 5002
- Order Service: 5003
- SQL Server: 1433
- RabbitMQ: 5672 (AMQP), 15672 (Management UI)

## Scalability Considerations

### Horizontal Scaling
- Each service can be scaled independently
- Stateless services enable easy scaling
- Load balancer can distribute requests

### Database Scaling
- Read replicas for read-heavy operations
- Database sharding possible per service
- Caching layer can be added (Redis)

### Message Queue Scaling
- RabbitMQ clustering for high availability
- Multiple consumers for parallel processing
- Dead letter queues for error handling

## Monitoring & Observability

### Health Checks
- `/health` endpoint on each service
- Database connectivity checks
- Service status monitoring

### Logging
- Built-in .NET logging
- Console and file logging
- Request/response logging
- Error tracking

### Future Enhancements
- Distributed tracing (OpenTelemetry)
- Metrics collection (Prometheus)
- Log aggregation (ELK Stack)
- APM tools (Application Insights)

## Technology Stack Summary

| Component | Technology |
|-----------|-----------|
| Runtime | .NET 8 |
| API Gateway | Ocelot |
| Database | SQL Server 2022 |
| Message Queue | RabbitMQ |
| Message Bus | MassTransit |
| Authentication | JWT Bearer Tokens |
| ORM | Entity Framework Core |
| Logging | .NET Built-in Logging |
| Containerization | Docker |
| Orchestration | Docker Compose |

## Best Practices Implemented

✅ **Microservices Principles**
- Single Responsibility
- Database per Service
- API Gateway Pattern
- Service Independence

✅ **Security**
- JWT Authentication
- Password Hashing
- Role-based Authorization
- Secure Configuration

✅ **Reliability**
- Health Checks
- Error Handling
- Structured Logging
- Event-driven Architecture

✅ **Scalability**
- Stateless Services
- Horizontal Scaling Ready
- Message Queue for Async Processing
- Independent Deployment

✅ **Maintainability**
- Clean Architecture
- Separation of Concerns
- Shared Contracts
- Comprehensive Documentation

