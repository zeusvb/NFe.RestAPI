# NFe.RestAPI

REST API para emissão e consulta de NFe/NFCe com .NET 8, PostgreSQL e Docker.

## Estrutura

```text
NFe.RestAPI/
├── src/
│   ├── NFe.RestAPI/
│   ├── NFe.Application/
│   ├── NFe.Domain/
│   └── NFe.Infrastructure/
├── tests/
│   └── NFe.Tests/
├── scripts/
│   └── init.sql
├── .github/workflows/
│   ├── ci.yml
│   └── cd.yml
├── Dockerfile
├── docker-compose.yml
└── NFe.RestAPI.sln
```

## Executando com Docker

```bash
docker compose up --build
```

API: `http://localhost:5000`
Health check: `http://localhost:5000/health`

## Build local

```bash
dotnet restore NFe.RestAPI.sln
dotnet build NFe.RestAPI.sln
```
