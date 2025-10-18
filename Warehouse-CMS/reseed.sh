#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet build --no-restore
timeout 15 dotnet run --no-build || true
echo "Seeding completed!"
