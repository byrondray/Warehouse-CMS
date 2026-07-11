FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY Warehouse-CMS/Warehouse-CMS.csproj Warehouse-CMS/
RUN dotnet restore Warehouse-CMS/Warehouse-CMS.csproj
COPY . .
RUN dotnet publish Warehouse-CMS/Warehouse-CMS.csproj -c Release -o /app/publish -p:ExcludeDevDeps=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
# ICU provides the culture data en-US currency formatting relies on (Alpine omits it by default).
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=build /app/publish .
ENV DOTNET_EnableDiagnostics=0
# PORT is injected by Railway at runtime; bind Kestrel to it via the entrypoint
# so the value is resolved at container start, not at image build time.
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-3000} dotnet Warehouse-CMS.dll"]
