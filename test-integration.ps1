#!/usr/bin/env pwsh
# Script para testar a integração ZeusFiscal

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Teste de Integração ZeusFiscal" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar build
Write-Host "[1/4] Compilando projeto..." -ForegroundColor Yellow
cd "C:\Fontes\NFe.RestAPI"
$buildResult = dotnet build 2>&1

if ($buildResult -match "Compilação bem-sucedida") {
	Write-Host "✅ Build bem-sucedido!" -ForegroundColor Green
} else {
	Write-Host "❌ Build falhou!" -ForegroundColor Red
	Write-Host $buildResult
	exit 1
}

Write-Host ""

# 2. Verificar se o pacote está instalado
Write-Host "[2/4] Verificando pacote Hercules.NET.Nfe.Nfce..." -ForegroundColor Yellow
$packagePath = "$env:USERPROFILE\.nuget\packages\hercules.net.nfe.nfce\2026.3.15.14"
if (Test-Path $packagePath) {
	Write-Host "✅ Pacote encontrado em: $packagePath" -ForegroundColor Green

	# Listar DLLs
	$dlls = Get-ChildItem "$packagePath\lib\net8.0" -Filter "*.dll"
	Write-Host "DLLs disponíveis:" -ForegroundColor Cyan
	foreach ($dll in $dlls) {
		Write-Host "  • $($dll.Name)" -ForegroundColor White
	}
} else {
	Write-Host "❌ Pacote não encontrado!" -ForegroundColor Red
	exit 1
}

Write-Host ""

# 3. Verificar configuração
Write-Host "[3/4] Verificando configuração..." -ForegroundColor Yellow
$appSettings = Get-Content "NFe.RestAPI\appsettings.json" | ConvertFrom-Json

if ($appSettings.ZeusFiscal) {
	Write-Host "✅ Seção ZeusFiscal encontrada!" -ForegroundColor Green
	Write-Host "Configuração:" -ForegroundColor Cyan
	Write-Host "  • Certificado: $($appSettings.ZeusFiscal.CertificatePath)" -ForegroundColor White
	Write-Host "  • CNPJ Emitente: $($appSettings.ZeusFiscal.EmitterCnpj)" -ForegroundColor White
	Write-Host "  • Ambiente: $($appSettings.ZeusFiscal.Environment)" -ForegroundColor White
	Write-Host "  • Timeout: $($appSettings.ZeusFiscal.TimeoutSeconds)s" -ForegroundColor White
} else {
	Write-Host "⚠️  Seção ZeusFiscal não encontrada!" -ForegroundColor Yellow
}

Write-Host ""

# 4. Verificar arquivos criados
Write-Host "[4/4] Verificando arquivos da implementação..." -ForegroundColor Yellow

$files = @(
	"NFe.Domain\Interfaces\IZeusFiscalNfeService.cs",
	"NFe.Infrastructure\ExternalServices\ZeusFiscalNfeService.cs",
	"NFe.Application\Services\NfeService.cs",
	"NFe.Application\Services\NfceService.cs",
	"ZeusFiscal_Implementation_Guide.md",
	"INTEGRATION_SUMMARY.md"
)

$allExists = $true
foreach ($file in $files) {
	if (Test-Path $file) {
		Write-Host "✅ $file" -ForegroundColor Green
	} else {
		Write-Host "❌ $file (NÃO ENCONTRADO)" -ForegroundColor Red
		$allExists = $false
	}
}

Write-Host ""

if ($allExists) {
	Write-Host "========================================" -ForegroundColor Green
	Write-Host "✅ TUDO PRONTO PARA USAR!" -ForegroundColor Green
	Write-Host "========================================" -ForegroundColor Green
	Write-Host ""
	Write-Host "Próximos passos:" -ForegroundColor Cyan
	Write-Host "1. Configure seu certificado em ./certificates/cert.pfx" -ForegroundColor White
	Write-Host "2. Atualize ZeusFiscal:EmitterCnpj e password em appsettings.json" -ForegroundColor White
	Write-Host "3. Inicie a aplicação: dotnet run --project NFe.RestAPI" -ForegroundColor White
	Write-Host "4. Teste em: https://localhost:5001/swagger" -ForegroundColor White
	Write-Host ""
} else {
	Write-Host "========================================" -ForegroundColor Yellow
	Write-Host "⚠️  ALGUNS ARQUIVOS FALTANDO" -ForegroundColor Yellow
	Write-Host "========================================" -ForegroundColor Yellow
	exit 1
}
