#!/bin/bash

# Exit on any failure
set -e

if [ -z "$SWAGGERHUB_API_KEY" ]
then
    echo "You must configure SWAGGERHUB_API_KEY"
    exit 1
fi

if [ ! -x "$(command -v npm)" ]
then
    echo "Node must be installed"
    exit 1
fi

if [ ! -x "$(command -v swaggerhub)" ]
then
    echo "Installing swaggerhub-cli"
    npm i -g --silent swaggerhub-cli
fi

owner="Hackney"
api_name="evidence-api"
api_version="v1.0"
swagger_version="1.0.0"

tmp_dir="$(mktemp -d)"
json_file="${tmp_dir}/swagger.json"

(
dotnet tool restore

dotnet build -c Release -o out
dotnet swagger tofile --output "${json_file}" ./out/EvidenceApi.dll "${api_version}"
)

swagger_path="${owner}/${api_name}/${swagger_version}"

if [ -z "$(swaggerhub api:get "${swagger_path}")" ]
then
    swaggerhub api:create "$swagger_path" --file "${json_file}"
else
    swaggerhub api:update "$swagger_path" --file "${json_file}"
fi

echo "Cleaning up ${tmp_dir}"
rm -rf "$tmp_dir"
