#!/usr/bin/env sh
set -eu

cert_dir="/https"
cert_name="loc.f3cms.com"
cert_pfx_path="$cert_dir/$cert_name.pfx"

if [ ! -f "$cert_pfx_path" ]; then
    echo "Missing development certificate: $cert_pfx_path" >&2
    echo "Run bin/build.sh first so mkcert can generate the certificate under conf/iis." >&2
    exit 1
fi

exec dotnet OntoCms.Web.dll