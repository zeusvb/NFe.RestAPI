using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;
using NFe.Domain.Entities;
using NFe.Domain.Interfaces;
using NFe.Infrastructure.Data;

namespace NFe.Application.Services
{
    public class NfeService : NFe.Application.Interfaces.INfeService
    {
        private readonly IZeusFiscalNfeService _zeusFiscalService;
        private readonly NfeDbContext _dbContext;
        private readonly ILogger<NfeService> _logger;

        public NfeService(IZeusFiscalNfeService zeusFiscalService, NfeDbContext dbContext, ILogger<NfeService> logger)
        {
            _zeusFiscalService = zeusFiscalService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<NfeResponse> EmitirNfeAsync(EmitirNfeRequest request)
        {
            try
            {
                _logger.LogInformation($"Iniciando emissão de NFe para CNPJ: {request.DestinationCnpj}");

                // Buscar empresa
                var company = await _dbContext.Companies.FindAsync(request.CompanyId);
                if (company == null)
                {
                    _logger.LogError($"Empresa não encontrada: {request.CompanyId}");
                    return new NfeResponse
                    {
                        Success = false,
                        Message = "Empresa não encontrada",
                        Status = "erro"
                    };
                }

                // Gerar número sequencial para a NF
                var nfeNumber = await GenerarNfeNumberAsync(request.CompanyId, request.Series);

                // Construir XML da NFe (será implementado em BuildNfeXmlAsync)
                // Por enquanto, usar um XML placeholder que será aprimorado depois
                var nfeXml = BuildNfeXmlRequest(request, company.Cnpj, nfeNumber);

                // Transmitir via ZeusFiscal
                var xmlResponse = await _zeusFiscalService.EmitirNfeAsync(company.Cnpj, nfeXml);

                // Parsear retorno do SEFAZ
                var nfeProc = XDocument.Parse(xmlResponse);
                var accessKey = ExtractAccessKey(nfeProc);
                var protocolNumber = ExtractProtocolNumber(nfeProc);
                var status = ExtractStatus(nfeProc);
                var isSuccessful = status == "100"; // 100 = Autorizado

                // Salvar no banco de dados
                var nfeDocument = new NfeDocument
                {
                    CompanyId = request.CompanyId,
                    NfeNumber = nfeNumber,
                    Series = request.Series,
                    ProtocolNumber = protocolNumber,
                    AccessKey = accessKey,
                    Status = isSuccessful ? "autorizada" : "rejeitada",
                    XmlContent = xmlResponse,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.NfeDocuments.Add(nfeDocument);
                await _dbContext.SaveChangesAsync();

                // Registrar evento
                var nfeEvent = new NfeEvent
                {
                    NfeId = nfeDocument.Id,
                    EventType = isSuccessful ? "autorizada" : "rejeitada",
                    Description = isSuccessful 
                        ? $"NFe autorizada pela SEFAZ. Protocolo: {protocolNumber}" 
                        : $"NFe rejeitada. Status: {status}",
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.NfeEvents.Add(nfeEvent);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"NFe {(isSuccessful ? "autorizada" : "rejeitada")}. Chave: {accessKey}");

                return new NfeResponse
                {
                    Success = isSuccessful,
                    Message = isSuccessful ? "NFe autorizada com sucesso" : $"NFe rejeitada. Status: {status}",
                    NfeNumber = nfeNumber,
                    Series = request.Series,
                    ProtocolNumber = protocolNumber,
                    AccessKey = accessKey,
                    Status = isSuccessful ? "autorizada" : "rejeitada",
                    XmlContent = xmlResponse,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao emitir NFe");
                return new NfeResponse
                {
                    Success = false,
                    Message = $"Erro ao emitir NFe: {ex.Message}",
                    Status = "erro"
                };
            }
        }

        public async Task<ConsultarNfeResponse> ConsultarNfeAsync(string accessKey)
        {
            try
            {
                _logger.LogInformation($"Consultando NFe: {accessKey}");

                // Buscar NF no banco para obter CNPJ do emitente
                var nfeDocument = await _dbContext.NfeDocuments.FindAsync(accessKey);
                if (nfeDocument == null)
                {
                    _logger.LogWarning($"NFe não encontrada no banco: {accessKey}");
                    return new ConsultarNfeResponse
                    {
                        Success = false,
                        Message = "NFe não encontrada no banco de dados",
                        AccessKey = accessKey
                    };
                }

                // Consultar via ZeusFiscal
                var xmlResponse = await _zeusFiscalService.ConsultarNfeAsync(nfeDocument.Company?.Cnpj ?? "", accessKey);

                // Parsear retorno
                var retorno = XDocument.Parse(xmlResponse);
                var status = ExtractConsultaStatus(retorno);
                var protocolNumber = ExtractConsultaProtocol(retorno);

                // Atualizar status no banco
                if (!string.IsNullOrWhiteSpace(status))
                {
                    nfeDocument.Status = MapStatusCode(status);
                    nfeDocument.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();

                    var nfeEvent = new NfeEvent
                    {
                        NfeId = nfeDocument.Id,
                        EventType = "consulta",
                        Description = $"Status consultado: {status}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.NfeEvents.Add(nfeEvent);
                    await _dbContext.SaveChangesAsync();
                }

                _logger.LogInformation($"NFe consultada com sucesso. Status: {status}");

                return new ConsultarNfeResponse
                {
                    Success = true,
                    Message = "Consulta realizada com sucesso",
                    AccessKey = accessKey,
                    Status = status,
                    ProtocolNumber = protocolNumber,
                    XmlContent = xmlResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao consultar NFe: {accessKey}");
                return new ConsultarNfeResponse
                {
                    Success = false,
                    Message = $"Erro ao consultar NFe: {ex.Message}",
                    AccessKey = accessKey
                };
            }
        }

        public async Task<NfeResponse> CancelarNfeAsync(int nfeId, string justificativa)
        {
            try
            {
                _logger.LogInformation($"Cancelando NFe: {nfeId}");

                var nfeDocument = await _dbContext.NfeDocuments.FindAsync(nfeId);
                if (nfeDocument == null)
                {
                    _logger.LogError($"NFe não encontrada: {nfeId}");
                    return new NfeResponse
                    {
                        Success = false,
                        Message = "NFe não encontrada",
                        Status = "erro"
                    };
                }

                if (string.IsNullOrWhiteSpace(justificativa) || justificativa.Length < 15)
                {
                    return new NfeResponse
                    {
                        Success = false,
                        Message = "Justificativa deve ter no mínimo 15 caracteres",
                        Status = "erro"
                    };
                }

                // Cancelar via ZeusFiscal
                var xmlResponse = await _zeusFiscalService.CancelarNfeAsync(
                    nfeDocument.Company?.Cnpj ?? "", 
                    nfeDocument.AccessKey, 
                    justificativa);

                // Parsear retorno
                var retorno = XDocument.Parse(xmlResponse);
                var cancelStatus = ExtractCancelStatus(retorno);
                var protocolNumber = ExtractCancelProtocol(retorno);
                var isSuccessful = cancelStatus == "135" || cancelStatus == "128"; // 135 = Cancelado, 128 = Cancelamento registrado

                // Atualizar status no banco
                nfeDocument.Status = isSuccessful ? "cancelada" : "pendente_cancelamento";
                nfeDocument.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                var nfeEvent = new NfeEvent
                {
                    NfeId = nfeDocument.Id,
                    EventType = isSuccessful ? "cancelada" : "cancelamento_rejeitado",
                    Description = $"Cancelamento: {(isSuccessful ? "Autorizado" : "Rejeitado")}. Justificativa: {justificativa}. Protocolo: {protocolNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.NfeEvents.Add(nfeEvent);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"NFe {(isSuccessful ? "cancelada" : "cancelamento rejeitado")}. Protocolo: {protocolNumber}");

                return new NfeResponse
                {
                    Success = isSuccessful,
                    Message = isSuccessful ? "NFe cancelada com sucesso" : $"Cancelamento rejeitado. Status: {cancelStatus}",
                    ProtocolNumber = protocolNumber,
                    AccessKey = nfeDocument.AccessKey,
                    Status = isSuccessful ? "cancelada" : "pendente_cancelamento"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao cancelar NFe: {nfeId}");
                return new NfeResponse
                {
                    Success = false,
                    Message = $"Erro ao cancelar NFe: {ex.Message}",
                    Status = "erro"
                };
            }
        }

        private async Task<string> GenerarNfeNumberAsync(int companyId, string series)
        {
            // TODO: Implementar lógica de geração sequencial de número de NFe usando banco de dados
            // Por enquanto, retorna um número randômico
            var random = new Random();
            return await Task.FromResult(random.Next(1, 999999).ToString().PadLeft(9, '0'));
        }

        /// <summary>
        /// Constrói um XML básico de NFe a partir do request
        /// </summary>
        private string BuildNfeXmlRequest(EmitirNfeRequest request, string emitterCnpj, string nfeNumber)
        {
            // PLACEHOLDER: Construir XML conforme manual de padrões técnicos da NF-e
            // Será aprimorado após análise completa da ZeusFiscal
            var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe versao=""4.00"">
        <ide>
            <cUF>35</cUF>
            <cNF>{nfeNumber}</cNF>
            <natOp>{request.NatureOfOperation ?? "VENDA"}</natOp>
            <mod>55</mod>
            <serie>{request.Series ?? "1"}</serie>
            <nNF>{nfeNumber}</nNF>
            <dEmi>{request.IssueDate:yyyyMMdd}</dEmi>
            <hEmi>{request.IssueDate:HHmmss}</hEmi>
            <idDest>1</idDest>
            <indFinal>1</indFinal>
            <indPres>1</indPres>
            <procEmi>0</procEmi>
            <verProc>5.0</verProc>
        </ide>
        <emit>
            <CNPJ>{emitterCnpj}</CNPJ>
        </emit>
        <dest>
            <CNPJ>{request.DestinationCnpj}</CNPJ>
            <xNome>{request.DestinationName}</xNome>
        </dest>
        <!-- Produtos/Serviços serão adicionados aqui -->
    </infNFe>
</NFe>";
            return xml;
        }

        /// <summary>
        /// Extrai a chave de acesso do XML de retorno
        /// </summary>
        private string ExtractAccessKey(XDocument nfeProc)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var infProt = nfeProc.Root?.Element(ns + "protNFe")?.Element(ns + "infProt");
                var chNFe = infProt?.Element(ns + "chNFe");
                return chNFe?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Extrai o número de protocolo do XML de retorno
        /// </summary>
        private string ExtractProtocolNumber(XDocument nfeProc)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var infProt = nfeProc.Root?.Element(ns + "protNFe")?.Element(ns + "infProt");
                var nProt = infProt?.Element(ns + "nProt");
                return nProt?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Extrai o status do XML de retorno (cStat)
        /// </summary>
        private string ExtractStatus(XDocument nfeProc)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var infProt = nfeProc.Root?.Element(ns + "protNFe")?.Element(ns + "infProt");
                var cStat = infProt?.Element(ns + "cStat");
                return cStat?.Value ?? "999";
            }
            catch
            {
                return "999";
            }
        }

        /// <summary>
        /// Extrai o status da consulta XML
        /// </summary>
        private string ExtractConsultaStatus(XDocument retorno)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var cStat = retorno.Root?.Element(ns + "cStat");
                return cStat?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Extrai o protocolo da consulta
        /// </summary>
        private string ExtractConsultaProtocol(XDocument retorno)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var nProt = retorno.Root?.Element(ns + "nProt");
                return nProt?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Extrai o status do cancelamento
        /// </summary>
        private string ExtractCancelStatus(XDocument retorno)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var infEvento = retorno.Root?.Element(ns + "infEvento");
                var cStat = infEvento?.Element(ns + "cStat");
                return cStat?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Extrai o protocolo do cancelamento
        /// </summary>
        private string ExtractCancelProtocol(XDocument retorno)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var infEvento = retorno.Root?.Element(ns + "infEvento");
                var nProt = infEvento?.Element(ns + "nProt");
                return nProt?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Mapeia código de status numérico para descrição legível
        /// </summary>
        private string MapStatusCode(string statusCode)
        {
            return statusCode switch
            {
                "100" => "autorizada",
                "150" => "uso_denied",
                "301" => "uso_confirmado",
                "302" => "uso_cancelado",
                "303" => "uso_denegado",
                "304" => "uso_operacional_nao_realizada",
                "305" => "cancelamento_nao_realizado",
                _ => "indefinido"
            };
        }
    }
}
