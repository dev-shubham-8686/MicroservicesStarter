#!/bin/bash

# Deployment Script for Microservices
# Usage: ./scripts/deploy.sh [environment] [version]

set -e  # Exit on error

ENVIRONMENT=${1:-staging}
VERSION=${2:-latest}
REGISTRY=${REGISTRY:-ghcr.io}

echo "🚀 Deploying to $ENVIRONMENT environment"
echo "Version: $VERSION"
echo "Registry: $REGISTRY"

# Load environment variables
if [ -f ".env.$ENVIRONMENT" ]; then
    echo "Loading .env.$ENVIRONMENT"
    export $(cat .env.$ENVIRONMENT | grep -v '^#' | xargs)
else
    echo "⚠️  Warning: .env.$ENVIRONMENT not found"
fi

# Set version
export VERSION=$VERSION
export REGISTRY=$REGISTRY

# Pull latest images
echo "📥 Pulling Docker images..."
docker-compose -f docker-compose.prod.yml pull

# Stop existing containers
echo "🛑 Stopping existing containers..."
docker-compose -f docker-compose.prod.yml down

# Start services
echo "▶️  Starting services..."
docker-compose -f docker-compose.prod.yml up -d

# Wait for services to be healthy
echo "⏳ Waiting for services to be healthy..."
sleep 30

# Health checks
echo "🏥 Running health checks..."

check_health() {
    local service=$1
    local port=$2
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if curl -f http://localhost:$port/health > /dev/null 2>&1; then
            echo "✅ $service is healthy"
            return 0
        fi
        echo "⏳ Waiting for $service... ($attempt/$max_attempts)"
        sleep 2
        attempt=$((attempt + 1))
    done
    
    echo "❌ $service failed health check"
    return 1
}

check_health "API Gateway" ${API_GATEWAY_PORT:-5000} || exit 1
check_health "Identity Service" ${IDENTITY_SERVICE_PORT:-5001} || exit 1
check_health "Product Service" ${PRODUCT_SERVICE_PORT:-5002} || exit 1
check_health "Order Service" ${ORDER_SERVICE_PORT:-5003} || exit 1

echo "✅ Deployment completed successfully!"
echo ""
echo "Services are running:"
echo "  - API Gateway: http://localhost:${API_GATEWAY_PORT:-5000}"
echo "  - Identity Service: http://localhost:${IDENTITY_SERVICE_PORT:-5001}"
echo "  - Product Service: http://localhost:${PRODUCT_SERVICE_PORT:-5002}"
echo "  - Order Service: http://localhost:${ORDER_SERVICE_PORT:-5003}"
echo "  - RabbitMQ Management: http://localhost:${RABBITMQ_MGMT_PORT:-15672}"

