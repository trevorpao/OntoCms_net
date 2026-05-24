#!/usr/bin/env bash

set -euo pipefail

CU_DIR="$(cd "$(dirname "$0")" && cd ../ && pwd)"
ENV_FILE="$CU_DIR/.env"
COMPOSE_FILE="$CU_DIR/conf/docker/docker-compose.yml"
PROJECT_NAME="ontocms_net"
CERT_DIR="$CU_DIR/conf/iis"
CERT_NAME="loc.f3cms.com"
CERT_PEM_PATH="$CERT_DIR/$CERT_NAME.pem"
CERT_KEY_PATH="$CERT_DIR/$CERT_NAME-key.pem"
CERT_PFX_PATH="$CERT_DIR/$CERT_NAME.pfx"

set -a
source "$ENV_FILE"
set +a

prepare_dev_certificate() {
	mkdir -p "$CERT_DIR"

	if [[ -f "$CERT_PFX_PATH" ]]; then
		echo "Using existing development certificate: $CERT_PFX_PATH"
		return
	fi

	if ! command -v mkcert >/dev/null 2>&1; then
		echo "mkcert is required to generate the local HTTPS certificate on macOS." >&2
		echo "Install it with: brew install mkcert nss" >&2
		exit 1
	fi

	if ! command -v openssl >/dev/null 2>&1; then
		echo "openssl is required to convert the mkcert certificate into PFX format." >&2
		exit 1
	fi

	echo "Installing mkcert local CA if needed..."
	mkcert -install

	echo "Generating development certificate in conf/iis ..."
	mkcert \
		-cert-file "$CERT_PEM_PATH" \
		-key-file "$CERT_KEY_PATH" \
		"$CERT_NAME" localhost 127.0.0.1 ::1

	openssl pkcs12 -export \
		-out "$CERT_PFX_PATH" \
		-inkey "$CERT_KEY_PATH" \
		-in "$CERT_PEM_PATH" \
		-passout "pass:${ONTOCMS_DEV_CERT_PASSWORD}"
}

prepare_dev_certificate

docker compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" -f "$COMPOSE_FILE" build web
docker compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps
