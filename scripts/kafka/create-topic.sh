#!/bin/bash
if [ $# -lt 1 ]; then
  echo "Usage: $0 TOPIC_NAME [PARTITIONS] [REPLICATION_FACTOR]"
  exit 1
fi

./scripts/kafka/kafka-manager.sh create-topic "$@"
