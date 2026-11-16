# ==============================================================================
# Multi-stage Dockerfile para EstudioContable.WebApi (.NET 8)
# ==============================================================================

# Etapa 1: Build y Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos de solución y proyectos para aprovechar la caché de capas de Docker
COPY EstudioContable.sln ./
COPY src/EstudioContable.Domain/EstudioContable.Domain.csproj src/EstudioContable.Domain/
COPY src/EstudioContable.Application/EstudioContable.Application.csproj src/EstudioContable.Application/
COPY src/EstudioContable.Infrastructure/EstudioContable.Infrastructure.csproj src/EstudioContable.Infrastructure/
COPY src/EstudioContable.WebApi/EstudioContable.WebApi.csproj src/EstudioContable.WebApi/

# Restaurar paquetes NuGet
RUN dotnet restore EstudioContable.sln

# Copiar el código fuente completo y compilar en modo Release
COPY src/ src/
WORKDIR /app/src/EstudioContable.WebApi
RUN dotnet publish EstudioContable.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime optimizado
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Crear directorio para almacenamiento de archivos subidos por tenants
RUN mkdir -p /app/storage && chmod 755 /app/storage

# Crear usuario no-root para mayor seguridad
RUN groupadd -r dotnetuser && useradd -r -g dotnetuser dotnetuser

# Copiar binarios compilados desde la etapa de compilación
COPY --from=build --chown=dotnetuser:dotnetuser /app/publish .

# Cambiar propietario del directorio de almacenamiento
RUN chown dotnetuser:dotnetuser /app/storage

# Variables de entorno por defecto
ENV ASPNETCORE_HTTP_PORTS=8080 \
    Storage__BasePath=/app/storage \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# Cambiar al usuario no-root
USER dotnetuser

# Healthcheck para monitoreo en producción
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD ["wget", "--no-verbose", "--tries=1", "--spider", "http://localhost:8080/health"]

ENTRYPOINT ["dotnet", "EstudioContable.WebApi.dll"]
