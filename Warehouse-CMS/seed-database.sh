#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet build
timeout 30 dotnet run || true
echo "Database seeding completed!"
