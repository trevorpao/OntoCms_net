#!/usr/bin/env bash

set -euo pipefail

PROJECT_PATH="/src/src/public/OntoCms.Web.csproj"
OUTPUT_DLL="/src/src/public/bin/Debug/net8.0/OntoCms.Web.dll"
ASSETS_FILE="/src/src/public/obj/project.assets.json"

needs_restore() {
	if [[ ! -f "$ASSETS_FILE" ]]; then
		return 0
	fi

	find \
		/src/src/public \
		\( -path '/src/src/public/bin' -o -path '/src/src/public/obj' \) -prune -o \
		-type f \
		\( -name '*.csproj' -o -name 'Directory.Packages.props' -o -name 'NuGet.Config' \) \
		-newer "$ASSETS_FILE" \
		-print -quit | grep -q .
}

needs_build() {
	if [[ ! -f "$OUTPUT_DLL" ]]; then
		return 0
	fi

	find \
		/src/src/public \
		/src/src/conventions \
		/src/src/Modules \
		/src/src/theme/default \
		/src/document/sql \
		\( -path '/src/src/public/bin' -o -path '/src/src/public/obj' \) -prune -o \
		-type f \
		\( -name '*.cs' -o -name '*.cshtml' -o -name '*.csproj' -o -name '*.json' -o -name '*.sql' \) \
		-newer "$OUTPUT_DLL" \
		-print -quit | grep -q .
}

if needs_build; then
	build_args=(build "$PROJECT_PATH" -c Debug -v minimal)
	if [[ -f "$ASSETS_FILE" ]] && ! needs_restore; then
		build_args+=(--no-restore)
	fi

	dotnet "${build_args[@]}"
fi

echo "Web Debug build is up to date."