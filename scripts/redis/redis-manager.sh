#!/bin/bash

# Redis CLI wrapper script
# Allows executing Redis commands directly

set -e

# Docker container name
REDIS_CONTAINER="dotnet-ecommerce-redis-1"

# Check if the Redis container is running
if ! docker ps | grep -q $REDIS_CONTAINER; then
echo "Error: Redis container ($REDIS_CONTAINER) is not running."
echo "Start the containers with: docker-compose up -d"
exit 1
fi

# If no arguments, enter interactive mode
if [ $# -eq 0 ]; then
echo "Entering Redis CLI interactive mode. Type 'exit' to quit."
docker exec -it $REDIS_CONTAINER redis-cli
else
# Otherwise, execute the provided command
docker exec -it $REDIS_CONTAINER redis-cli "$@"
fi