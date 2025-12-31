# Complete Docker & Architecture Guide
## From Beginner to Advanced

This comprehensive guide explains everything about Docker, Docker Compose, and the complete architecture of this microservices application. Whether you're new to Docker or an experienced developer, this guide will help you understand how everything works together.

---

## Table of Contents

1. [Docker Fundamentals](#docker-fundamentals)
2. [Understanding Dockerfiles](#understanding-dockerfiles)
3. [Docker Compose Deep Dive](#docker-compose-deep-dive)
4. [Architecture Overview](#architecture-overview)
5. [Service-by-Service Breakdown](#service-by-service-breakdown)
6. [Configuration Explained](#configuration-explained)
7. [How Everything Works Together](#how-everything-works-together)
8. [Advanced Topics](#advanced-topics)
9. [Troubleshooting](#troubleshooting)

---

## Docker Fundamentals

### What is Docker?

**Docker** is a platform that allows you to package applications and their dependencies into lightweight, portable containers. Think of a container as a shipping container for software - it contains everything your application needs to run, regardless of where it's deployed.

### Key Docker Concepts

#### 1. **Container**
- A lightweight, standalone, executable package that includes everything needed to run an application
- Isolated from other containers and the host system
- Runs on top of the host operating system's kernel

#### 2. **Image**
- A read-only template used to create containers
- Like a blueprint or recipe
- Built from a Dockerfile

#### 3. **Dockerfile**
- A text file containing instructions for building a Docker image
- Defines what goes into your container

#### 4. **Docker Compose**
- A tool for defining and running multi-container Docker applications
- Uses a YAML file to configure services
- Simplifies managing multiple containers

### Why Use Docker?

✅ **Consistency**: "Works on my machine" becomes "works everywhere"  
✅ **Isolation**: Each service runs in its own environment  
✅ **Portability**: Run the same container on any Docker host  
✅ **Scalability**: Easy to scale services up or down  
✅ **Resource Efficiency**: Containers share the OS kernel  

---

## Understanding Dockerfiles

### What is a Dockerfile?

A **Dockerfile** is a script that contains instructions for building a Docker image. Each instruction creates a layer in the image, and these layers are cached for faster rebuilds.

### Our Dockerfile Structure (Multi-Stage Build)

All our service Dockerfiles use a **multi-stage build** pattern, which is a best practice for creating smaller, more efficient images. Let's break down a typical Dockerfile:

```dockerfile
# Stage 1: Base Image (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
```

**Explanation:**
- `FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base`: 
  - Uses the official .NET 8 runtime image as the base
  - This image contains only what's needed to RUN the application (smaller size)
  - `AS base` gives this stage a name for reference later
- `WORKDIR /app`: Sets the working directory inside the container
- `EXPOSE 80` and `EXPOSE 443`: Documents which ports the container will use (doesn't actually open them)

```dockerfile
# Stage 2: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/gateway/ApiGateway/ApiGateway.csproj", "src/gateway/ApiGateway/"]
COPY ["src/shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/shared/Shared.Infrastructure/"]
COPY ["src/shared/Shared.Contracts/Shared.Contracts.csproj", "src/shared/Shared.Contracts/"]
RUN dotnet restore "src/gateway/ApiGateway/ApiGateway.csproj"
```

**Explanation:**
- `FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build`: 
  - Uses the .NET 8 SDK image (includes compiler, build tools)
  - Larger image, but needed for building
- `COPY ["source", "destination"]`: 
  - Copies project files first (before source code)
  - This is a Docker optimization: dependency files change less frequently
  - Docker caches layers, so if dependencies don't change, it reuses the cache
- `RUN dotnet restore`: Downloads NuGet packages

```dockerfile
COPY . .
WORKDIR "/src/src/gateway/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build
```

**Explanation:**
- `COPY . .`: Copies all source code
- `WORKDIR`: Changes to the service directory
- `RUN dotnet build`: Compiles the application in Release mode

```dockerfile
# Stage 3: Publish Stage
FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false
```

**Explanation:**
- `FROM build AS publish`: Uses the build stage as base
- `dotnet publish`: Creates the final, optimized application package
- `/p:UseAppHost=false`: Disables creating a platform-specific executable (we use `dotnet` command instead)

```dockerfile
# Stage 4: Final Runtime Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
```

**Explanation:**
- `FROM base AS final`: Goes back to the small runtime image
- `COPY --from=publish /app/publish .`: 
  - Copies only the published files from the publish stage
  - `--from=publish` means "copy from the publish stage"
  - This keeps the final image small (no SDK, no source code, just the compiled app)
- `ENV ASPNETCORE_URLS=http://+:80`: 
  - Sets environment variable for ASP.NET Core
  - `+:80` means "listen on all network interfaces on port 80"
- `ENTRYPOINT ["dotnet", "ApiGateway.dll"]`: 
  - Command that runs when container starts
  - `["dotnet", "ApiGateway.dll"]` is the exec form (recommended)

### Why Multi-Stage Builds?

**Without multi-stage:**
- Final image size: ~1.5 GB (includes SDK, build tools, source code)

**With multi-stage:**
- Final image size: ~200 MB (only runtime and compiled app)
- Faster deployments
- Better security (no build tools in production)

### Dockerfile Commands Reference

| Command | Purpose | Example |
|---------|---------|---------|
| `FROM` | Base image to start from | `FROM mcr.microsoft.com/dotnet/aspnet:8.0` |
| `WORKDIR` | Set working directory | `WORKDIR /app` |
| `COPY` | Copy files into image | `COPY . .` |
| `RUN` | Execute command during build | `RUN dotnet restore` |
| `ENV` | Set environment variable | `ENV ASPNETCORE_URLS=http://+:80` |
| `EXPOSE` | Document port usage | `EXPOSE 80` |
| `ENTRYPOINT` | Command to run when container starts | `ENTRYPOINT ["dotnet", "app.dll"]` |

---

## Docker Compose Deep Dive

### What is Docker Compose?

**Docker Compose** is a tool for defining and running multi-container Docker applications. Instead of running multiple `docker run` commands, you define everything in a YAML file and use `docker-compose up`.

### Our docker-compose.yml Structure

Let's break down our `docker-compose.yml` file section by section:

#### 1. Services Section

Each service in Docker Compose represents a container. We have 6 services:

1. **sqlserver** - Database server
2. **rabbitmq** - Message broker
3. **identityservice** - Authentication service
4. **productservice** - Product management service
5. **orderservice** - Order processing service
6. **apigateway** - API Gateway

#### 2. SQL Server Service

```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  container_name: sqlserver
  environment:
    - ACCEPT_EULA=Y
    - SA_PASSWORD=YourStrong@Passw0rd
    - MSSQL_PID=Developer
  ports:
    - "1433:1433"
  volumes:
    - sqlserver_data:/var/opt/mssql
  networks:
    - microservices-network
  restart: unless-stopped
  healthcheck:
    test: ["CMD-SHELL", "exit 0"]
    interval: 30s
    timeout: 5s
    retries: 3
    start_period: 90s
```

**Detailed Explanation:**

- `image: mcr.microsoft.com/mssql/server:2022-latest`
  - Uses the official Microsoft SQL Server 2022 image
  - `latest` tag means it will use the most recent version

- `container_name: sqlserver`
  - Gives the container a specific name
  - Other services can reference it by this name
  - Without this, Docker generates a random name

- `environment:`
  - `ACCEPT_EULA=Y`: Accepts Microsoft's End User License Agreement (required)
  - `SA_PASSWORD=YourStrong@Passw0rd`: Sets the system administrator password
    - ⚠️ **Security Note**: Change this in production!
    - Must meet complexity requirements (uppercase, lowercase, number, special char)
  - `MSSQL_PID=Developer`: Uses Developer edition (free for development)

- `ports:`
  - `"1433:1433"`: Maps host port 1433 to container port 1433
  - Format: `"HOST_PORT:CONTAINER_PORT"`
  - Allows external access to SQL Server

- `volumes:`
  - `sqlserver_data:/var/opt/mssql`: Creates a named volume
  - Data persists even if container is removed
  - `/var/opt/mssql` is where SQL Server stores data inside the container

- `networks:`
  - `microservices-network`: Connects to our custom network
  - Services on the same network can communicate by service name

- `restart: unless-stopped`
  - Automatically restarts container if it crashes
  - Won't restart if manually stopped
  - Options: `no`, `always`, `on-failure`, `unless-stopped`

- `healthcheck:`
  - Monitors container health
  - `test`: Command to check health (simple exit 0 for now)
  - `interval: 30s`: Check every 30 seconds
  - `timeout: 5s`: Wait max 5 seconds for response
  - `retries: 3`: Mark unhealthy after 3 failures
  - `start_period: 90s`: Give SQL Server 90 seconds to start before checking

#### 3. RabbitMQ Service

```yaml
rabbitmq:
  image: rabbitmq:3-management
  container_name: rabbitmq
  ports:
    - "5672:5672"    # AMQP protocol port
    - "15672:15672"  # Management UI port
  environment:
    - RABBITMQ_DEFAULT_USER=guest
    - RABBITMQ_DEFAULT_PASS=guest
  volumes:
    - rabbitmq_data:/var/lib/rabbitmq
  networks:
    - microservices-network
  restart: unless-stopped
  healthcheck:
    test: rabbitmq-diagnostics -q ping || exit 1
    interval: 10s
    timeout: 5s
    retries: 20
    start_period: 30s
```

**Detailed Explanation:**

- `image: rabbitmq:3-management`
  - Uses RabbitMQ version 3 with management plugin
  - Management plugin provides web UI

- `ports:`
  - `5672:5672`: AMQP protocol port (for message queuing)
  - `15672:15672`: Management UI (web interface)
    - Access at: http://localhost:15672
    - Default login: guest/guest

- `environment:`
  - `RABBITMQ_DEFAULT_USER=guest`: Default username
  - `RABBITMQ_DEFAULT_PASS=guest`: Default password
    - ⚠️ Change in production!

- `healthcheck:`
  - `test: rabbitmq-diagnostics -q ping`: Uses RabbitMQ's built-in health check
  - More sophisticated than SQL Server's check

#### 4. Identity Service

```yaml
identityservice:
  build:
    context: .
    dockerfile: src/services/IdentityService/Dockerfile
  container_name: identityservice
  ports:
    - "5001:80"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=IdentityDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Connect Timeout=30;
    - Jwt__Key=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
    - Jwt__Issuer=MicroservicesStarter
    - Jwt__Audience=MicroservicesStarter
    - RabbitMQ__Host=rabbitmq
    - RabbitMQ__Username=guest
    - RabbitMQ__Password=guest
  depends_on:
    sqlserver:
      condition: service_started
  networks:
    - microservices-network
  restart: unless-stopped
```

**Detailed Explanation:**

- `build:` (instead of `image:`)
  - Builds the image from a Dockerfile
  - `context: .`: Build context (root directory)
  - `dockerfile: src/services/IdentityService/Dockerfile`: Path to Dockerfile

- `ports:`
  - `"5001:80"`: Maps host port 5001 to container port 80
  - Access service at: http://localhost:5001

- `environment:`
  - `ASPNETCORE_ENVIRONMENT=Development`: Sets .NET environment
  - `ConnectionStrings__DefaultConnection=...`:
    - `Server=sqlserver`: Uses service name (Docker DNS resolution)
    - `Database=IdentityDb`: Database name
    - `User Id=sa`: SQL Server admin user
    - `Password=YourStrong@Passw0rd`: SQL Server password
    - `TrustServerCertificate=True`: Skips SSL certificate validation (dev only)
    - `Connect Timeout=30`: Wait 30 seconds for connection
  - `Jwt__Key=...`: JWT signing key (must be 32+ characters)
  - `Jwt__Issuer` and `Jwt__Audience`: JWT token validation settings
  - `RabbitMQ__Host=rabbitmq`: Uses service name for RabbitMQ

- `depends_on:`
  - `sqlserver: condition: service_started`
  - Waits for SQL Server to start before starting this service
  - `service_started` means "wait until container starts" (not necessarily healthy)
  - Services handle connection retries internally

- **Note**: Notice we use service names (`sqlserver`, `rabbitmq`) instead of `localhost`. Docker Compose creates a DNS that resolves service names to container IPs.

#### 5. Product Service

```yaml
productservice:
  build:
    context: .
    dockerfile: src/services/ProductService/Dockerfile
  container_name: productservice
  ports:
    - "5002:80"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=ProductDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Connect Timeout=30;
    - Jwt__Key=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
    - Jwt__Issuer=MicroservicesStarter
    - Jwt__Audience=MicroservicesStarter
    - RabbitMQ__Host=rabbitmq
    - RabbitMQ__Username=guest
    - RabbitMQ__Password=guest
  depends_on:
    sqlserver:
      condition: service_started
    rabbitmq:
      condition: service_started
  networks:
    - microservices-network
  restart: unless-stopped
```

**Key Differences from Identity Service:**
- Port `5002` instead of `5001`
- Database: `ProductDb` instead of `IdentityDb`
- Depends on both `sqlserver` AND `rabbitmq` (publishes events)

#### 6. Order Service

Similar to Product Service but:
- Port `5003`
- Database: `OrderDb`
- Consumes events from RabbitMQ

#### 7. API Gateway

```yaml
apigateway:
  build:
    context: .
    dockerfile: src/gateway/ApiGateway/Dockerfile
  container_name: apigateway
  ports:
    - "5000:80"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - Jwt__Key=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
    - Jwt__Issuer=MicroservicesStarter
    - Jwt__Audience=MicroservicesStarter
  depends_on:
    - identityservice
    - productservice
    - orderservice
  networks:
    - microservices-network
  restart: unless-stopped
```

**Key Points:**
- Port `5000` (main entry point)
- No database connection (routes to other services)
- Depends on all three microservices
- Needs JWT settings to validate tokens

#### 8. Networks Section

```yaml
networks:
  microservices-network:
    driver: bridge
```

**Explanation:**
- Creates a custom Docker network
- `bridge` driver: Default network driver
- All services on this network can communicate by service name
- Isolated from other Docker networks

#### 9. Volumes Section

```yaml
volumes:
  sqlserver_data:
  rabbitmq_data:
```

**Explanation:**
- Creates named volumes for data persistence
- `sqlserver_data`: Stores SQL Server database files
- `rabbitmq_data`: Stores RabbitMQ data
- Data survives container removal/recreation
- Managed by Docker (stored in Docker's volume directory)

---

## Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Applications                       │
│              (Web, Mobile, Postman, etc.)                    │
└────────────────────────┬────────────────────────────────────┘
                         │
                         │ HTTP Requests
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              API Gateway (Ocelot)                            │
│              Port: 5000                                      │
│  • Single Entry Point                                        │
│  • Request Routing                                           │
│  • Authentication/Authorization                              │
│  • Load Balancing (ready)                                    │
└─────┬──────────────┬──────────────┬────────────────────────┘
      │              │              │
      │ /api/auth    │ /api/product │ /api/order
      │              │              │
      ▼              ▼              ▼
┌──────────┐  ┌──────────┐  ┌──────────┐
│Identity  │  │ Product  │  │  Order   │
│ Service  │  │ Service  │  │ Service  │
│ Port:    │  │ Port:    │  │ Port:    │
│  5001    │  │  5002    │  │  5003    │
│          │  │          │  │          │
│ Identity │  │ Product  │  │  Order   │
│   Db     │  │   Db     │  │   Db     │
└────┬─────┘  └────┬─────┘  └────┬─────┘
     │             │              │
     │             │              │
     └─────────────┴──────────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │   SQL Server          │
        │   Port: 1433          │
        │   • IdentityDb        │
        │   • ProductDb         │
        │   • OrderDb           │
        └───────────────────────┘

        ┌───────────────────────┐
        │   RabbitMQ            │
        │   Ports: 5672, 15672  │
        │   • Event Bus         │
        │   • Message Queue     │
        │   • Async Comm        │
        └───────────────────────┘
```

### Communication Patterns

#### 1. **Synchronous Communication (HTTP/REST)**
- Client → API Gateway → Microservice
- Direct request/response
- Used for: User actions, CRUD operations

#### 2. **Asynchronous Communication (RabbitMQ)**
- Service → RabbitMQ → Other Services
- Event-driven, decoupled
- Used for: Product creation events, Order events

---

## Service-by-Service Breakdown

### 1. SQL Server Service

**Purpose:** Centralized database server hosting all databases

**Configuration:**
- **Image:** `mcr.microsoft.com/mssql/server:2022-latest`
- **Port:** 1433 (standard SQL Server port)
- **Databases:**
  - `IdentityDb` - User accounts, roles
  - `ProductDb` - Product catalog
  - `OrderDb` - Orders and order items

**Why Separate Databases?**
- **Database per Service** pattern
- Each service owns its data
- Prevents tight coupling
- Allows independent scaling

**Data Persistence:**
- Volume: `sqlserver_data`
- Data stored in: `/var/opt/mssql` (inside container)
- Survives container restarts

**Access:**
- From host: `localhost:1433`
- From containers: `sqlserver:1433` (service name)
- Credentials: `sa` / `YourStrong@Passw0rd`

### 2. RabbitMQ Service

**Purpose:** Message broker for event-driven communication

**Configuration:**
- **Image:** `rabbitmq:3-management`
- **Ports:**
  - `5672` - AMQP protocol (message queuing)
  - `15672` - Management UI (web interface)

**Features:**
- Message queuing
- Event publishing/subscribing
- Decouples services
- Enables async processing

**Management UI:**
- URL: http://localhost:15672
- Username: `guest`
- Password: `guest`
- View queues, exchanges, connections

**How It Works:**
1. Service publishes event to RabbitMQ
2. RabbitMQ routes to subscribed consumers
3. Consumers process events asynchronously

### 3. Identity Service

**Purpose:** Authentication and authorization

**Responsibilities:**
- User registration
- User login
- JWT token generation
- Password hashing (BCrypt)
- Role management (Admin, User)

**Database:** `IdentityDb`
- Tables: `Users`, `Roles`, `UserRoles`

**Endpoints:**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get token

**JWT Configuration:**
- Signs tokens with secret key
- Sets issuer and audience
- Other services validate tokens using same settings

**Port:** 5001 (direct access) or through Gateway at 5000

### 4. Product Service

**Purpose:** Product catalog management

**Responsibilities:**
- CRUD operations for products
- Stock management
- Publishes `ProductCreatedEvent` to RabbitMQ

**Database:** `ProductDb`
- Table: `Products`

**Endpoints:**
- `GET /api/product` - Get all products (authenticated)
- `POST /api/product` - Create product (Admin only)
- `GET /api/product/{id}` - Get product by ID
- `PUT /api/product/{id}` - Update product (Admin)
- `DELETE /api/product/{id}` - Delete product (Admin)

**Event Publishing:**
- When product created → publishes `ProductCreatedEvent`
- Order Service consumes this event

**Port:** 5002

### 5. Order Service

**Purpose:** Order processing

**Responsibilities:**
- Order creation
- Order management
- Consumes `ProductCreatedEvent` from RabbitMQ
- Publishes `OrderCreatedEvent` to RabbitMQ

**Database:** `OrderDb`
- Tables: `Orders`, `OrderItems`

**Endpoints:**
- `POST /api/order` - Create order
- `GET /api/order/my-orders` - Get user's orders
- `GET /api/order/{id}` - Get order by ID

**Event Consumption:**
- Listens for `ProductCreatedEvent`
- Can react to product changes

**Port:** 5003

### 6. API Gateway

**Purpose:** Single entry point for all requests

**Technology:** Ocelot

**Responsibilities:**
- Route requests to appropriate services
- Aggregate responses
- Handle authentication
- Load balancing (ready)

**Routing:**
- `/api/auth/*` → Identity Service (5001)
- `/api/product/*` → Product Service (5002)
- `/api/order/*` → Order Service (5003)

**Port:** 5000 (main entry point)

**Configuration:** `ocelot.json` defines routes

---

## Configuration Explained

### Environment Variables

Environment variables in Docker Compose use double underscores (`__`) to represent nested configuration in .NET.

#### Connection String Format

```yaml
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=IdentityDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Connect Timeout=30;
```

**Breakdown:**
- `ConnectionStrings__DefaultConnection`: Maps to `ConnectionStrings.DefaultConnection` in appsettings.json
- `Server=sqlserver`: Database server (uses Docker service name)
- `Database=IdentityDb`: Database name
- `User Id=sa`: SQL Server admin user
- `Password=YourStrong@Passw0rd`: SQL Server password
- `TrustServerCertificate=True`: Skip SSL validation (dev only)
- `Connect Timeout=30`: Wait 30 seconds for connection

#### JWT Configuration

```yaml
Jwt__Key=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
Jwt__Issuer=MicroservicesStarter
Jwt__Audience=MicroservicesStarter
```

**Breakdown:**
- `Jwt__Key`: Secret key for signing tokens (must be 32+ characters)
- `Jwt__Issuer`: Who issued the token
- `Jwt__Audience`: Who the token is intended for
- All services must use the SAME values for token validation

#### RabbitMQ Configuration

```yaml
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest
```

**Breakdown:**
- `RabbitMQ__Host`: RabbitMQ server (uses Docker service name)
- `RabbitMQ__Username`: RabbitMQ username
- `RabbitMQ__Password`: RabbitMQ password

### Port Mapping

Format: `"HOST_PORT:CONTAINER_PORT"`

- **Host Port:** Port on your machine (localhost)
- **Container Port:** Port inside the container

Examples:
- `"5000:80"` - Access container's port 80 via localhost:5000
- `"1433:1433"` - Access container's port 1433 via localhost:1433

### Volume Mapping

Format: `VOLUME_NAME:/path/in/container`

- **Named Volume:** Managed by Docker, persists data
- **Path:** Where data is stored inside container

Examples:
- `sqlserver_data:/var/opt/mssql` - SQL Server data
- `rabbitmq_data:/var/lib/rabbitmq` - RabbitMQ data

---

## How Everything Works Together

### Startup Sequence

When you run `docker-compose up`, here's what happens:

1. **Docker Compose reads docker-compose.yml**
   - Identifies all services
   - Creates network: `microservices-network`
   - Creates volumes: `sqlserver_data`, `rabbitmq_data`

2. **Infrastructure Services Start First**
   - SQL Server starts (no dependencies)
   - RabbitMQ starts (no dependencies)

3. **Microservices Start (in parallel, respecting depends_on)**
   - Identity Service waits for SQL Server
   - Product Service waits for SQL Server and RabbitMQ
   - Order Service waits for SQL Server and RabbitMQ

4. **API Gateway Starts Last**
   - Waits for all microservices
   - Routes requests to services

### Request Flow Examples

#### Example 1: User Registration

```
1. Client sends POST to http://localhost:5000/api/auth/register
   {
     "email": "user@example.com",
     "password": "Password123!",
     "firstName": "John",
     "lastName": "Doe"
   }

2. API Gateway (port 5000) receives request
   - Checks routing rules in ocelot.json
   - Routes to Identity Service

3. Identity Service (port 5001) processes request
   - Validates input
   - Hashes password with BCrypt
   - Connects to SQL Server (sqlserver:1433)
   - Creates user in IdentityDb
   - Generates JWT token

4. Response flows back:
   Identity Service → API Gateway → Client
   
5. Client receives:
   {
     "token": "eyJhbGciOiJIUzI1NiIs...",
     "user": { ... }
   }
```

#### Example 2: Create Product (with Event Publishing)

```
1. Client sends POST to http://localhost:5000/api/product
   Authorization: Bearer {token}
   {
     "name": "Laptop",
     "price": 999.99,
     "stock": 50
   }

2. API Gateway validates JWT token
   - Extracts user info
   - Checks if user is Admin

3. Product Service creates product
   - Connects to SQL Server
   - Saves to ProductDb
   - Publishes ProductCreatedEvent to RabbitMQ

4. RabbitMQ routes event
   - Order Service is subscribed
   - Receives ProductCreatedEvent
   - Can update its local cache/data

5. Response: Product created successfully
```

#### Example 3: Create Order

```
1. Client sends POST to http://localhost:5000/api/order
   Authorization: Bearer {token}
   {
     "items": [
       {
         "productId": "123",
         "quantity": 2,
         "price": 999.99
       }
     ]
   }

2. API Gateway routes to Order Service

3. Order Service:
   - Validates JWT token
   - Extracts user ID from token
   - Creates order in OrderDb
   - Publishes OrderCreatedEvent to RabbitMQ

4. Other services can consume OrderCreatedEvent
   - Inventory service (if exists)
   - Notification service (if exists)
   - Analytics service (if exists)
```

### Network Communication

**Inside Docker Network:**
- Services communicate using service names
- Example: `sqlserver:1433` resolves to SQL Server container's IP
- DNS resolution handled by Docker

**From Host Machine:**
- Use `localhost` with mapped ports
- Example: `localhost:5000` → API Gateway

**Service Discovery:**
- Docker Compose creates DNS entries for each service
- Service name = hostname
- No need for IP addresses

---

## Advanced Topics

### 1. Multi-Stage Builds (Advanced)

**Why use multi-stage builds?**

**Traditional Single-Stage:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY . .
RUN dotnet publish -c Release
ENTRYPOINT ["dotnet", "app.dll"]
```
- Final image: ~1.5 GB (includes SDK, build tools)
- Security risk: Build tools in production
- Slower deployments

**Multi-Stage (Our Approach):**
```dockerfile
# Build stage (large, has SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... build steps ...

# Runtime stage (small, only runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "app.dll"]
```
- Final image: ~200 MB (only runtime)
- No build tools in production
- Faster deployments

### 2. Layer Caching Optimization

Docker caches layers. If a layer hasn't changed, it reuses the cache.

**Optimized Order:**
```dockerfile
# 1. Copy dependency files first (changes less frequently)
COPY ["*.csproj", "./"]
RUN dotnet restore

# 2. Copy source code (changes frequently)
COPY . .
RUN dotnet build
```

**Why this order?**
- If source code changes, Docker reuses cached restore layer
- Faster rebuilds

### 3. Health Checks

**Purpose:** Monitor container health

**Types:**
1. **Simple:** `exit 0` (always healthy)
2. **Command-based:** Run actual health check command
3. **HTTP-based:** `curl http://localhost/health`

**Our Implementation:**
```yaml
healthcheck:
  test: ["CMD-SHELL", "exit 0"]
  interval: 30s      # Check every 30 seconds
  timeout: 5s        # Wait max 5 seconds
  retries: 3         # Mark unhealthy after 3 failures
  start_period: 60s  # Give service 60s to start
```

**Advanced Health Check (Example):**
```yaml
healthcheck:
  test: ["CMD-SHELL", "curl -f http://localhost/health || exit 1"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 60s
```

### 4. Dependency Management

**depends_on Options:**

```yaml
depends_on:
  service_name:
    condition: service_started  # Wait until container starts
    # OR
    condition: service_healthy   # Wait until health check passes
```

**Our Approach:**
- Use `service_started` (faster)
- Services handle connection retries internally
- More resilient to timing issues

### 5. Environment Variable Precedence

Environment variables can be set in multiple places (highest to lowest priority):

1. **docker-compose.yml** (environment section)
2. **.env file** (in same directory)
3. **appsettings.json** (default values)

### 6. Volume Types

**Named Volumes (Our Approach):**
```yaml
volumes:
  sqlserver_data:
```
- Managed by Docker
- Persists data
- Can be backed up/restored

**Bind Mounts (Alternative):**
```yaml
volumes:
  - ./data:/var/opt/mssql
```
- Maps to host directory
- Good for development
- Not recommended for production

### 7. Network Isolation

**Custom Network (Our Approach):**
```yaml
networks:
  microservices-network:
    driver: bridge
```
- Isolated from other Docker networks
- Services can communicate by name
- Better security

**Default Network:**
- All services on default network
- Less isolation
- Not recommended for production

### 8. Resource Limits (Production)

Add resource limits to prevent one service from consuming all resources:

```yaml
services:
  identityservice:
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
```

### 9. Secrets Management (Production)

**Never hardcode secrets in docker-compose.yml!**

**Use Docker Secrets:**
```yaml
services:
  identityservice:
    secrets:
      - jwt_key
    environment:
      - Jwt__Key_FILE=/run/secrets/jwt_key

secrets:
  jwt_key:
    external: true
```

**Or use .env file (not committed to git):**
```bash
# .env file
JWT_KEY=YourSuperSecretKey
SQL_PASSWORD=YourStrongPassword
```

Then in docker-compose.yml:
```yaml
environment:
  - Jwt__Key=${JWT_KEY}
  - SA_PASSWORD=${SQL_PASSWORD}
```

### 10. Docker Compose Override Files

Create `docker-compose.override.yml` for local development:

```yaml
# docker-compose.override.yml (not committed to git)
services:
  identityservice:
    volumes:
      - ./src/services/IdentityService:/app  # Mount source code
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

Docker Compose automatically merges with `docker-compose.yml`.

---

## Troubleshooting

### Common Issues and Solutions

#### 1. Services Won't Start

**Problem:** Containers exit immediately

**Solutions:**
```bash
# Check logs
docker-compose logs [service-name]

# Check if ports are in use
netstat -an | findstr :5000  # Windows
lsof -i :5000                 # Linux/Mac

# Restart services
docker-compose restart [service-name]
```

#### 2. Database Connection Errors

**Problem:** "Cannot connect to SQL Server"

**Solutions:**
```bash
# Check SQL Server is running
docker-compose ps sqlserver

# Check SQL Server logs
docker-compose logs sqlserver

# Verify connection string uses service name
# Should be: Server=sqlserver (not localhost)

# Wait longer for SQL Server to start
# SQL Server takes 30-90 seconds to initialize
```

#### 3. RabbitMQ Connection Errors

**Problem:** "Cannot connect to RabbitMQ"

**Solutions:**
```bash
# Check RabbitMQ is running
docker-compose ps rabbitmq

# Access management UI
# http://localhost:15672 (guest/guest)

# Check RabbitMQ logs
docker-compose logs rabbitmq

# Verify hostname is 'rabbitmq' (not localhost)
```

#### 4. JWT Token Validation Fails

**Problem:** "Invalid token" errors

**Solutions:**
- Ensure all services use the SAME JWT key
- Check `Jwt__Key` environment variable matches
- Verify token hasn't expired
- Check issuer and audience match

#### 5. Port Already in Use

**Problem:** "Port is already allocated"

**Solutions:**
```bash
# Find process using port (Windows)
netstat -ano | findstr :5000

# Kill process (Windows)
taskkill /PID [process-id] /F

# Find process using port (Linux/Mac)
lsof -i :5000

# Kill process (Linux/Mac)
kill -9 [process-id]

# Or change port in docker-compose.yml
ports:
  - "5001:80"  # Change 5001 to available port
```

#### 6. Volume Permission Issues

**Problem:** "Permission denied" when accessing volumes

**Solutions:**
```bash
# Remove volume and recreate
docker-compose down -v
docker-compose up -d

# Check volume permissions
docker volume inspect microservicesstarter_sqlserver_data
```

#### 7. Build Failures

**Problem:** Docker build fails

**Solutions:**
```bash
# Clean build (no cache)
docker-compose build --no-cache

# Check Dockerfile syntax
# Ensure all COPY paths are correct

# Verify .NET SDK is available
docker pull mcr.microsoft.com/dotnet/sdk:8.0
```

#### 8. Services Can't Communicate

**Problem:** Services can't reach each other

**Solutions:**
```bash
# Verify all services on same network
docker network inspect microservicesstarter_microservices-network

# Test connectivity from container
docker exec -it identityservice ping sqlserver

# Check service names match
# Use service names, not container names
```

### Useful Docker Commands

```bash
# View running containers
docker-compose ps

# View logs
docker-compose logs [service-name]
docker-compose logs -f [service-name]  # Follow logs

# Restart service
docker-compose restart [service-name]

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Rebuild specific service
docker-compose build [service-name]

# Execute command in container
docker exec -it [container-name] /bin/bash

# View container resource usage
docker stats

# Clean up unused resources
docker system prune -a
```

---

## Best Practices Summary

### ✅ Do's

1. **Use multi-stage builds** for smaller images
2. **Use named volumes** for data persistence
3. **Use service names** for inter-service communication
4. **Set resource limits** in production
5. **Use health checks** for monitoring
6. **Keep secrets out** of docker-compose.yml
7. **Use .env files** for environment-specific config
8. **Document ports** and configurations
9. **Use depends_on** for startup ordering
10. **Test locally** before deploying

### ❌ Don'ts

1. **Don't hardcode passwords** in docker-compose.yml
2. **Don't use latest tag** in production (use specific versions)
3. **Don't expose unnecessary ports**
4. **Don't run as root** in containers (if possible)
5. **Don't store secrets** in images
6. **Don't ignore health checks**
7. **Don't use localhost** for inter-service communication
8. **Don't commit .env files** to git
9. **Don't skip dependency management**
10. **Don't forget to update** base images

---

## Quick Reference

### Ports

| Service | Port | URL |
|---------|------|-----|
| API Gateway | 5000 | http://localhost:5000 |
| Identity Service | 5001 | http://localhost:5001 |
| Product Service | 5002 | http://localhost:5002 |
| Order Service | 5003 | http://localhost:5003 |
| SQL Server | 1433 | localhost:1433 |
| RabbitMQ AMQP | 5672 | localhost:5672 |
| RabbitMQ UI | 15672 | http://localhost:15672 |

### Service Names (Internal)

| Service | Internal Hostname |
|---------|------------------|
| SQL Server | `sqlserver` |
| RabbitMQ | `rabbitmq` |
| Identity Service | `identityservice` |
| Product Service | `productservice` |
| Order Service | `orderservice` |
| API Gateway | `apigateway` |

### Default Credentials

| Service | Username | Password |
|---------|----------|----------|
| SQL Server | `sa` | `YourStrong@Passw0rd` |
| RabbitMQ | `guest` | `guest` |

⚠️ **Change these in production!**

---

## Conclusion

This guide covered:

1. ✅ Docker fundamentals and concepts
2. ✅ Dockerfile structure and multi-stage builds
3. ✅ Docker Compose configuration explained
4. ✅ Complete architecture overview
5. ✅ Service-by-service breakdown
6. ✅ Configuration values explained
7. ✅ How everything works together
8. ✅ Advanced topics and best practices
9. ✅ Troubleshooting guide

You should now understand:
- How Docker containers work
- How Dockerfiles build images
- How Docker Compose orchestrates services
- How services communicate
- How to configure and troubleshoot

**Next Steps:**
1. Experiment with docker-compose commands
2. Modify configurations and see what happens
3. Add new services
4. Implement health checks
5. Set up production configurations

Happy Dockerizing! 🐳

