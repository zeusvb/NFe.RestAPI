# Guia de Implementação Real - ZeusFiscal Integration

## 📋 Status Atual

✅ **Estrutura base implementada e compilando com sucesso!**

- ✅ Biblioteca `Hercules.NET.Nfe.Nfce` v2026.3.15.14 instalada
- ✅ Interface `IZeusFiscalNfeService` criada em `NFe.Domain.Interfaces`
- ✅ Adapter `ZeusFiscalNfeService` implementado em `NFe.Infrastructure.ExternalServices`
- ✅ Métodos principais: EmitirNfe, EmitirNfce, ConsultarNfe, CancelarNfe, HealthCheck
- ✅ NfeService e NfceService refatorados para usar o adapter
- ✅ DI registrada em Program.cs
- ✅ Configuração em appsettings.json com suporte a ZeusFiscal
- ✅ Build compilando sem erros

## 🔧 Como Completar a Implementação

### Passo 1: Explorar os Tipos Reais

Os tipos estão nas seguintes DLLs (instaladas via NuGet):
- `NFe.Classes.dll` - Classes de domínio (nfe, nfeProc, NfeProduto, etc.)
- `NFe.Servicos.dll` - Serviços de transmissão (NfeService, etc.)
- `NFe.Utils.dll` - Utilitários (FuncoesXml, etc.)
- `DFe.Utils.dll` - Utilitários gerais (ConfiguracaoServico, etc.)

**Localização no NuGet:**
```
C:\Users\[YourUser]\.nuget\packages\hercules.net.nfe.nfce\2026.3.15.14\lib\net8.0\
```

### Passo 2: Adicionar Using Statements Corretos

No arquivo `NFe.Infrastructure\ExternalServices\ZeusFiscalNfeService.cs`, descomente e ajuste:

```csharp
using NFe.Classes;           // Para nfe, nfeProc, NfeProduto
using NFe.Servicos;           // Para NfeService
using NFe.Utils;              // Para FuncoesXml
using DFe.Utils;              // Para ConfiguracaoServico, etc.
```

### Passo 3: Implementar o Método `ExecutarTransmissaoNfeAsync`

Substitua o placeholder em `ZeusFiscalNfeService.cs`:

```csharp
private async Task<string> ExecutarTransmissaoNfeAsync(string emitterCnpj, string xmlRequest, X509Certificate2 certificate)
{
	try
	{
		// 1. Parsear XML para objeto nfe
		var nfeObj = FuncoesXml.XmlStringParaClasse<nfe>(xmlRequest);

		// 2. Configurar serviço
		var config = ConfigurarServicoNfe(certificate, emitterCnpj);
		var nfeService = new NfeService(config);

		// 3. Transmitir
		nfeService.EnviarSincrono(nfeObj);

		// 4. Capturar resultado
		var procXml = nfeService.ObterXmlProcessado();

		return await Task.FromResult(procXml);
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Erro na transmissão NFe");
		throw;
	}
}
```

### Passo 4: Implementar o Método `ConfigurarServicoNfe`

Exemplo (ajuste conforme API real da ZeusFiscal):

```csharp
private ConfiguracaoServico ConfigurarServicoNfe(X509Certificate2 certificate, string emitterCnpj)
{
	var config = new ConfiguracaoServico();

	// Configurar baseado nos tipos reais disponíveis
	// config.Ambiente = _environment == "producao" ? ... : ...;
	// config.Certificado = new ConfiguracaoCertificado(certificate);
	// config.CpfCnpj = emitterCnpj;
	// config.TimeoutServico = ...;

	return config;
}
```

### Passo 5: Implementar Outros Métodos

Repita o mesmo padrão para:
- `ExecutarConsultaAsync` - use `nfeService.ConsultarStatusNfe(accessKey)`
- `ExecutarCancelamentoAsync` - use `nfeService.CancelarNfe(accessKey, justification)`

## 📚 Recursos Úteis

### Exemplo do ZeusFiscal GitHub

A biblioteca possui exemplos em:
- `NFe.AppTeste` - Projeto WPF com exemplos de emissão
- `NFe.AppTeste.NetCore` - Projeto Console para .NET 6+

Referência: https://github.com/Hercules-NET/ZeusFiscal

### Utilitários da Biblioteca

```csharp
// Serialização/Desserialização
var nfe = FuncoesXml.XmlStringParaClasse<nfe>(xmlString);
var xmlString = nfe.ObterXmlString();

// Validação
var validar = nfe.Validar();

// Assinatura digital
var assinado = assinador.Assinar(nfe);
```

## ⚠️ Pontos Importantes

1. **Certificado**: O arquivo PFX deve estar no caminho configurado em `appsettings.json`
2. **Ambiente**: Configurar "homologacao" ou "producao" corretamente
3. **Timeout**: Ajustar conforme necessário (padrão 30s)
4. **Logging**: Use `_logger` para debug - muitas informações úteis nos logs
5. **Tratamento de Erros**: A ZeusFiscal pode lançar exceções específicas - trate-as apropriadamente

## 🧪 Como Testar

1. Configurar certificado válido em `./certificates/cert.pfx`
2. Definir `ZeusFiscal:EmitterCnpj` no appsettings
3. Chamar endpoint de emissão via API
4. Verificar logs para erros/sucesso

## 📝 Próximos Passos

- [ ] Explorar tipos reais das DLLs
- [ ] Implementar métodos de transmissão
- [ ] Adicionar testes unitários
- [ ] Implementar retry/resilience
- [ ] Adicionar suporte a assinatura digital
- [ ] Implementar geração de DANFE (com QuestPdf ou FastReport)

---

**Nota**: Os placeholders retornam XMLs mockados para permitir testes iniciais da API sem certificado real. Substitua-os pela implementação real assim que determinar os tipos corretos.
