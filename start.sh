#!/bin/bash

# Bash script to start the microservices solution

echo "========================================"
echo "Microservices Starter - Startup Script"
echo "========================================"
echo ""

# Check if Docker is running
echo "Checking Docker..."
if ! docker ps > /dev/null 2>&1; then
    echo "Error: Docker is not running. Please start Docker."
    exit 1
fi

echo "Docker is running"
echo ""
echo "Starting all services with Docker Compose..."
docker-compose up -d

echo ""
echo "Waiting for services to be ready..."
sleep 30

echo ""
echo "Service Status:"
docker-compose ps

echo ""
echo "========================================"
echo "Services are starting up!"
echo "========================================"
echo ""
echo "Access points:"
echo "  API Gateway:     http://localhost:5000"
echo "  Identity Service: http://localhost:5001/swagger"
echo "  Product Service:  http://localhost:5002/swagger"
echo "  Order Service:    http://localhost:5003/swagger"
echo "  RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo ""
echo "To view logs: docker-compose logs -f [service-name]"
echo "To stop: docker-compose down"
echo ""


