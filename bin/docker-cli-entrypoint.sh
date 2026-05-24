#!/usr/bin/env bash

set -euo pipefail

PROJECT_PATH="/src/src/cli/OntoCms.Cli.csproj"
OUTPUT_DLL="/src/src/cli/bin/Debug/net8.0/OntoCms.Cli.dll"
ASSETS_FILE="/src/src/cli/obj/project.assets.json"

needs_build() {
	if [[ ! -f "$OUTPUT_DLL" ]]; then
		return 0
	fi

	find \
		/src/src/cli \
		/src/src/conventions \
		/src/src/Modules \
		/src/document/sql \
		-type f \
		\( -name '*.cs' -o -name '*.csproj' -o -name '*.sql' \) \
		-newer "$OUTPUT_DLL" \
		-print -quit | grep -q .
}

if needs_build; then
	build_args=(build "$PROJECT_PATH" -c Debug -v minimal)
	if [[ -f "$ASSETS_FILE" ]]; then
		build_args+=(--no-restore)
	fi

	dotnet "${build_args[@]}"
fi

exec dotnet "$OUTPUT_DLL" "$@"