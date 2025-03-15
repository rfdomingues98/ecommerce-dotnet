#!/bin/bash
if [ $# -lt 1 ]; then
  echo "Usage: $0 TOPIC_NAME"
  exit 1
fi

./scripts/kafka/kafka-manager.sh delete-topic "$@"
