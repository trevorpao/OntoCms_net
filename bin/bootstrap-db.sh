#!/usr/bin/env bash

set -euo pipefail

CU_DIR="$(cd "$(dirname "$0")" && cd ../ && pwd)"
ENV_FILE="$CU_DIR/.env"
COMPOSE_FILE="$CU_DIR/conf/docker/docker-compose.yml"
PROJECT_NAME="ontocms_net"

docker compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d db cli
docker compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T cli bash /src/bin/docker-cli-entrypoint.sh db:bootstrap