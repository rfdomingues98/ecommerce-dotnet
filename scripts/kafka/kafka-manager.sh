#!/bin/bash

# Kafka Management Script
# A utility script to manage Kafka operations

set -e

# Docker container name
KAFKA_CONTAINER="dotnet-ecommerce-kafka-1"

# Check if the Kafka container is running
check_kafka_running() {
if ! docker ps | grep -q $KAFKA_CONTAINER; then
echo "Error: Kafka container ($KAFKA_CONTAINER) is not running."
echo "Start the containers with: docker-compose up -d"
exit 1
fi
}

# Display help message
show_help() {
echo "Kafka Management Utility"
echo "========================"
echo "Usage: $0 [command]"
echo ""
echo "Commands:"
echo " list-topics List all Kafka topics"
echo " create-topic NAME Create a topic with name NAME"
echo " delete-topic NAME Delete a topic with name NAME"
echo " describe-topic NAME Show details of topic NAME"
echo " produce-to NAME Produce messages to topic NAME"
echo " consume-from NAME Consume messages from topic NAME"
echo " list-groups List consumer groups"
echo " describe-group NAME Describe consumer group NAME"
echo " status Show Kafka cluster status"
echo " reset-group NAME TOPIC Reset consumer group NAME for TOPIC"
echo " help Show this help message"
echo ""
}

# Main script logic
if [ $# -eq 0 ]; then
show_help
exit 0
fi

check_kafka_running

case "$1" in
list-topics)
echo "Listing Kafka topics..."
docker exec -it $KAFKA_CONTAINER kafka-topics --list --bootstrap-server localhost:9092
;;

create-topic)
if [ -z "$2" ]; then
echo "Error: Topic name is required"
echo "Usage: $0 create-topic TOPIC_NAME [PARTITIONS] [REPLICATION_FACTOR]"
exit 1
fi

TOPIC_NAME=$2
PARTITIONS=${3:-1}
REPLICATION=${4:-1}

echo "Creating topic '$TOPIC_NAME' with $PARTITIONS partition(s) and replication factor $REPLICATION..."
docker exec -it $KAFKA_CONTAINER kafka-topics --create \
--topic $TOPIC_NAME \
--bootstrap-server localhost:9092 \
--partitions $PARTITIONS \
--replication-factor $REPLICATION
;;

delete-topic)
if [ -z "$2" ]; then
echo "Error: Topic name is required"
echo "Usage: $0 delete-topic TOPIC_NAME"
exit 1
fi

TOPIC_NAME=$2
echo "Deleting topic '$TOPIC_NAME'..."
docker exec -it $KAFKA_CONTAINER kafka-topics --delete \
--topic $TOPIC_NAME \
--bootstrap-server localhost:9092
;;

describe-topic)
if [ -z "$2" ]; then
echo "Error: Topic name is required"
echo "Usage: $0 describe-topic TOPIC_NAME"
exit 1
fi

TOPIC_NAME=$2
echo "Describing topic '$TOPIC_NAME'..."
docker exec -it $KAFKA_CONTAINER kafka-topics --describe \
--topic $TOPIC_NAME \
--bootstrap-server localhost:9092
;;

produce-to)
if [ -z "$2" ]; then
echo "Error: Topic name is required"
echo "Usage: $0 produce-to TOPIC_NAME"
exit 1
fi

TOPIC_NAME=$2
echo "Starting producer for topic '$TOPIC_NAME'..."
echo "Type messages and press Enter. Press Ctrl+C to exit."
docker exec -it $KAFKA_CONTAINER kafka-console-producer \
--topic $TOPIC_NAME \
--bootstrap-server localhost:9092
;;

consume-from)
if [ -z "$2" ]; then
echo "Error: Topic name is required"
echo "Usage: $0 consume-from TOPIC_NAME [--from-beginning]"
exit 1
fi

TOPIC_NAME=$2
FROM_BEGINNING=""

if [ "$3" == "--from-beginning" ]; then
FROM_BEGINNING="--from-beginning"
fi

echo "Starting consumer for topic '$TOPIC_NAME'..."
echo "Press Ctrl+C to exit."
docker exec -it $KAFKA_CONTAINER kafka-console-consumer \
--topic $TOPIC_NAME \
--bootstrap-server localhost:9092 \
$FROM_BEGINNING
;;

list-groups)
echo "Listing consumer groups..."
docker exec -it $KAFKA_CONTAINER kafka-consumer-groups \
--list \
--bootstrap-server localhost:9092
;;

describe-group)
if [ -z "$2" ]; then
echo "Error: Group name is required"
echo "Usage: $0 describe-group GROUP_NAME"
exit 1
fi

GROUP_NAME=$2
echo "Describing consumer group '$GROUP_NAME'..."
docker exec -it $KAFKA_CONTAINER kafka-consumer-groups \
--describe \
--group $GROUP_NAME \
--bootstrap-server localhost:9092
;;

reset-group)
if [ -z "$2" ] || [ -z "$3" ]; then
echo "Error: Group name and topic are required"
echo "Usage: $0 reset-group GROUP_NAME TOPIC_NAME [--to-earliest|--to-latest|--to-offset OFFSET|--to-datetime
DATETIME|--shift-by N]"
exit 1
fi

GROUP_NAME=$2
TOPIC_NAME=$3
RESET_OPTION=${4:-"--to-earliest"}
RESET_VALUE=$5

RESET_CMD="$RESET_OPTION"
if [ ! -z "$RESET_VALUE" ]; then
RESET_CMD="$RESET_OPTION $RESET_VALUE"
fi

echo "Resetting offsets for consumer group '$GROUP_NAME' on topic '$TOPIC_NAME' with $RESET_CMD..."
docker exec -it $KAFKA_CONTAINER kafka-consumer-groups \
--reset-offsets \
--group $GROUP_NAME \
--topic $TOPIC_NAME \
--bootstrap-server localhost:9092 \
--execute \
$RESET_CMD
;;

status)
echo "Checking Kafka cluster status..."
docker exec -it $KAFKA_CONTAINER kafka-metadata-shell \
--bootstrap-server localhost:9092
;;

help)
show_help
;;

*)
echo "Error: Unknown command '$1'"
show_help
exit 1
;;
esac