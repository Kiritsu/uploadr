#!/bin/bash

set -e

dotnet tool install --global dotnet-ef
export PATH="$PATH:/root/.dotnet/tools"
export UPLOADR_PATH="./publish/uploadr.json"

#until dotnet ef database update --no-build; do
#>&2 echo "Waiting for Postgres"
#sleep 1
#done

>&2 echo "Postgres has started, running app"
exec dotnet ./publish/UploadR.dll
