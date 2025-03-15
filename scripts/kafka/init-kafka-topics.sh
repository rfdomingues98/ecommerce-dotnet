#!/bin/bash

# Script to initialize Kafka topics for the e-commerce application

set -e

echo "Initializing Kafka topics for e-commerce application..."

# Core business event topics
./scripts/kafka/create-topic.sh inventory-events 3 1
./scripts/kafka/create-topic.sh order-events 3 1
./scripts/kafka/create-topic.sh product-events 3 1
./scripts/kafka/create-topic.sh payment-events 3 1

# Notification topics
./scripts/kafka/create-topic.sh email-notifications 1 1
./scripts/kafka/create-topic.sh sms-notifications 1 1
./scripts/kafka/create-topic.sh push-notifications 1 1

# Dead letter queue topics
./scripts/kafka/create-topic.sh inventory-events-dlq 1 1
./scripts/kafka/create-topic.sh order-events-dlq 1 1

echo "Done initializing Kafka topics!"
echo "Topics created:"
./scripts/kafka/list-topics.sh