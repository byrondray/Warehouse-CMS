# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Warehouse-CMS/Warehouse-CMS.csproj", "Warehouse-CMS/"]
RUN dotnet restore "Warehouse-CMS/Warehouse-CMS.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Warehouse-CMS"
RUN dotnet build "Warehouse-CMS.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Warehouse-CMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published files
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Warehouse-CMS.dll"]
