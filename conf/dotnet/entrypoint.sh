#!/usr/bin/env sh
set -eu

cert_dir="/https"
cert_name="loc.f3cms.com"
cert_password="${ONTOCMS_DEV_CERT_PASSWORD:-OntoCms_https_2026!}"
cert_key_path="$cert_dir/$cert_name.key"
cert_crt_path="$cert_dir/$cert_name.crt"
cert_pfx_path="$cert_dir/$cert_name.pfx"

mkdir -p "$cert_dir"

if [ ! -f "$cert_pfx_path" ]; then
    openssl req -x509 -nodes -newkey rsa:2048 \
        -keyout "$cert_key_path" \
        -out "$cert_crt_path" \
        -days 3650 \
        -subj "/CN=$cert_name"

    openssl pkcs12 -export \
        -out "$cert_pfx_path" \
        -inkey "$cert_key_path" \
        -in "$cert_crt_path" \
        -passout "pass:$cert_password"
fi

exec dotnet OntoCms.Web.dll