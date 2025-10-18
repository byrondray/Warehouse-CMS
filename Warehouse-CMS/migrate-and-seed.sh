#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet ef database update --no-build
