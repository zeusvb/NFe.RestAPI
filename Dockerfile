# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY NFe.RestAPI.sln .

# Copy project files
COPY src/NFe.RestAPI/NFe.RestAPI.csproj src/NFe.RestAPI/
COPY src/NFe.Domain/NFe.Domain.csproj src/NFe.Domain/
COPY src/NFe.Application/NFe.Application.csproj src/NFe.Application/
COPY src/NFe.Infrastructure/NFe.Infrastructure.csproj src/NFe.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ src/

# Build
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install required libraries for DANFE printing
RUN apt-get update && \
    apt-get install -y --no-install-recommends libgdiplus libc6-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Create certificate and logs directories
RUN mkdir -p /app/certificates /app/logs

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD dotnet /app/Health.dll || exit 1

ENTRYPOINT ["dotnet", "NFe.RestAPI.dll"]