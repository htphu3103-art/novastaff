# =====================================================
# STAGE 1: BUILD
# =====================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files trước để tận dụng cache

COPY Shared/NovaStaff.Shared.csproj Shared/
COPY NovaStaff.Model/NovaStaff.Models.csproj NovaStaff.Model/
COPY NovaStaff.DataLayes/NovaStaff.DataLayers.csproj NovaStaff.DataLayes/
COPY NovaStaff.BusinessLayers/NovaStaff.BusinessLayers.csproj NovaStaff.BusinessLayers/
COPY NovaStaff.Infrastructure/NovaStaff.Infrastructure.csproj NovaStaff.Infrastructure/
COPY NovaStaff.Api/NovaStaff.API.csproj NovaStaff.Api/

COPY NovaStaff.sln .

# Restore dependencies

RUN dotnet restore NovaStaff.sln

# Copy toàn bộ source

COPY . .

# Publish

RUN dotnet publish NovaStaff.Api/NovaStaff.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# =====================================================
# STAGE 2: RUNTIME
# =====================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "NovaStaff.API.dll"]