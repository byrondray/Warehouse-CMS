FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY publish_output/ .
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-3000}
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "Warehouse-CMS.dll"]
