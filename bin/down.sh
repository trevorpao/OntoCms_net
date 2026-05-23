#!/usr/bin/env bash

set -euo pipefail

CU_DIR="$(cd "$(dirname "$0")/.." && pwd)"
ENV_FILE="$CU_DIR/.env"
COMPOSE_FILE="$CU_DIR/conf/docker/docker-compose.yml"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" stop
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps
