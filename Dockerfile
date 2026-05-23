# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore (layer-cached until csproj files change)
COPY src/AudioBatchConverter.Core/AudioBatchConverter.Core.csproj \
     src/AudioBatchConverter.Core/
COPY src/AudioBatchConverter.Worker/AudioBatchConverter.Worker.csproj \
     src/AudioBatchConverter.Worker/
RUN dotnet restore src/AudioBatchConverter.Worker/AudioBatchConverter.Worker.csproj

# Build
COPY src/AudioBatchConverter.Core/  src/AudioBatchConverter.Core/
COPY src/AudioBatchConverter.Worker/ src/AudioBatchConverter.Worker/
RUN dotnet publish src/AudioBatchConverter.Worker/AudioBatchConverter.Worker.csproj \
    -c Release -o /app --no-restore

# Run stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AudioBatchConverter.Worker.dll"]
