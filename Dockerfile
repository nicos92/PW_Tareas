# ========================= Etapa 1: build =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar la solución y los .csproj primero para aprovechar la caché de capas
COPY ["PW_Tareas.sln", "./"]
COPY ["src/Tareas.Domain/Tareas.Domain.csproj", "src/Tareas.Domain/"]
COPY ["src/Tareas.Application/Tareas.Application.csproj", "src/Tareas.Application/"]
COPY ["src/Tareas.Infrastructure/Tareas.Infrastructure.csproj", "src/Tareas.Infrastructure/"]
COPY ["src/Tareas.Web/Tareas.Web.csproj", "src/Tareas.Web/"]
RUN dotnet restore "src/Tareas.Web/Tareas.Web.csproj"

# Copiar el resto del código y publicar en Release
COPY . .
RUN dotnet publish "src/Tareas.Web/Tareas.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ========================= Etapa 2: runtime =======================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Usuario sin privilegios (uid 1000, recomendado por Hugging Face Spaces)
RUN useradd -m -u 1000 user

# La app debe escuchar en el 8080 (local y Hugging Face vía app_port)
ENV ASPNETCORE_URLS=http://+:8080
# Carpeta de datos persistente dentro del contenedor (SQLite)
ENV DATA_DIR=/app/data

# Copiar la aplicación publicada
COPY --chown=user:user --from=build /app/publish .

# Carpetas escribibles: base de datos y subida de imágenes
RUN mkdir -p /app/data /app/wwwroot/uploads \
    && chown -R user:user /app/data /app/wwwroot/uploads

EXPOSE 8080

USER user

# Las migraciones EF Core se aplican automáticamente al arrancar
ENTRYPOINT ["dotnet", "Tareas.Web.dll"]
