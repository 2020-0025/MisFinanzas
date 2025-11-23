# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos del proyecto
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el código
COPY . ./
RUN dotnet publish -c Release -o out

# Verificar que las fuentes se copiaron
RUN echo "Verificando fuentes en build..." && \
    ls -la /app/out/wwwroot/fonts/ || echo "Directorio de fuentes NO encontrado en build"

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Instalar libgdiplus para PdfSharpCore
RUN apt-get update && \
    apt-get install -y libgdiplus && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

    # Copiar aplicación desde build
COPY --from=build /app/out .

# Copiar fuentes explícitamente (doble seguridad)
COPY --from=build /app/wwwroot/fonts ./wwwroot/fonts

# Verificar que las fuentes están presentes
RUN echo "Contenido de wwwroot/fonts:" && \
    ls -la /app/wwwroot/fonts/ || echo "ERROR: Directorio de fuentes NO encontrado"


# Exponer el puerto que usa Render
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Comando de inicio
ENTRYPOINT ["dotnet", "MisFinanzas.dll"]