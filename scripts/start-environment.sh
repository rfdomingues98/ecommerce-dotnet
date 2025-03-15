#!/bin/bash

# Script to start the entire e-commerce application environment

set -e

echo "Starting e-commerce application environment..."

# Start all services with Docker Compose
echo "Starting Docker containers..."
docker-compose up -d

# Wait for Kafka to be ready
echo "Waiting for Kafka to be ready..."
sleep 15

# Initialize Kafka topics
echo "Initializing Kafka topics..."
./scripts/kafka/init-kafka-topics.sh

echo "Environment is ready!"
echo ""
echo "Available services:"
echo "- Kafka UI:           http://localhost:8080"
echo "- Redis Commander:    http://localhost:8082" 
echo "- Inventory API:      http://localhost:5101"
echo "- Order API:          http://localhost:5102"
echo "- Product API:        http://localhost:5103"
echo "- Notification API:   http://localhost:5104"