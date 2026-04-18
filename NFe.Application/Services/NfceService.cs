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
    public class NfceService : INfceService
    {
        private readonly IZeusFiscalNfeService _zeusFiscalService;
        private readonly NfeDbContext _dbContext;
        private readonly ILogger<NfceService> _logger;

        public NfceService(IZeusFiscalNfeService zeusFiscalService, NfeDbContext dbContext, ILogger<NfceService> logger)
        {
            _zeusFiscalService = zeusFiscalService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<NfeResponse> EmitirNfceAsync(EmitirNfeRequest request)
        {
            try
            {
                _logger.LogInformation($"Iniciando emissão de NFCe para CNPJ: {request.DestinationCnpj}");

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

                // Gerar número sequencial para a NFCe
                var nfceNumber = await GenerarNfceNumberAsync(request.CompanyId, request.Series);

                // Construir XML da NFCe
                var nfceXml = BuildNfceXmlRequest(request, company.Cnpj, nfceNumber);

                // Transmitir via ZeusFiscal
                var xmlResponse = await _zeusFiscalService.EmitirNfceAsync(company.Cnpj, nfceXml);

                // Parsear retorno do SEFAZ
                var nfceProc = XDocument.Parse(xmlResponse);
                var accessKey = ExtractAccessKey(nfceProc);
                var protocolNumber = ExtractProtocolNumber(nfceProc);
                var status = ExtractStatus(nfceProc);
                var isSuccessful = status == "100"; // 100 = Autorizado

                // Salvar no banco de dados
                var nfeDocument = new NfeDocument
                {
                    CompanyId = request.CompanyId,
                    NfeNumber = nfceNumber,
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
                        ? $"NFCe autorizada pela SEFAZ. Protocolo: {protocolNumber}" 
                        : $"NFCe rejeitada. Status: {status}",
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.NfeEvents.Add(nfeEvent);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"NFCe {(isSuccessful ? "autorizada" : "rejeitada")}. Chave: {accessKey}");

                return new NfeResponse
                {
                    Success = isSuccessful,
                    Message = isSuccessful ? "NFCe autorizada com sucesso" : $"NFCe rejeitada. Status: {status}",
                    NfeNumber = nfceNumber,
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
                _logger.LogError(ex, "Erro ao emitir NFCe");
                return new NfeResponse
                {
                    Success = false,
                    Message = $"Erro ao emitir NFCe: {ex.Message}",
                    Status = "erro"
                };
            }
        }

        public async Task<ConsultarNfeResponse> ConsultarNfceAsync(string accessKey)
        {
            try
            {
                _logger.LogInformation($"Consultando NFCe: {accessKey}");

                // Buscar NFCe no banco para obter CNPJ do emitente
                var nfeDocument = await _dbContext.NfeDocuments.FindAsync(accessKey);
                if (nfeDocument == null)
                {
                    _logger.LogWarning($"NFCe não encontrada no banco: {accessKey}");
                    return new ConsultarNfeResponse
                    {
                        Success = false,
                        Message = "NFCe não encontrada no banco de dados",
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

                _logger.LogInformation($"NFCe consultada com sucesso. Status: {status}");

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
                _logger.LogError(ex, $"Erro ao consultar NFCe: {accessKey}");
                return new ConsultarNfeResponse
                {
                    Success = false,
                    Message = $"Erro ao consultar NFCe: {ex.Message}",
                    AccessKey = accessKey
                };
            }
        }

        private async Task<string> GenerarNfceNumberAsync(int companyId, string series)
        {
            // TODO: Implementar lógica de geração sequencial de número de NFCe
            // Por enquanto, retorna um número randômico
            var random = new Random();
            return await Task.FromResult(random.Next(1, 999999).ToString().PadLeft(9, '0'));
        }

        /// <summary>
        /// Constrói um XML básico de NFCe a partir do request
        /// </summary>
        private string BuildNfceXmlRequest(EmitirNfeRequest request, string emitterCnpj, string nfceNumber)
        {
            // PLACEHOLDER: Construir XML de NFCe conforme manual técnico
            var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <infNFe versao=""4.00"">
        <ide>
            <cUF>35</cUF>
            <cNF>{nfceNumber}</cNF>
            <natOp>{request.NatureOfOperation ?? "VENDA"}</natOp>
            <mod>65</mod>
            <serie>{request.Series ?? "1"}</serie>
            <nNF>{nfceNumber}</nNF>
            <dEmi>{request.IssueDate:yyyyMMdd}</dEmi>
            <hEmi>{request.IssueDate:HHmmss}</hEmi>
            <idDest>1</idDest>
            <indFinal>0</indFinal>
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
        private string ExtractAccessKey(XDocument nfceProc)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var infProt = nfceProc.Root?.Element(ns + "protNFe")?.Element(ns + "infProt");
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
        private string ExtractProtocolNumber(XDocument nfceProc)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var infProt = nfceProc.Root?.Element(ns + "protNFe")?.Element(ns + "infProt");
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
        private string ExtractStatus(XDocument nfceProc)
        {
            try
            {
                var ns = XNamespace.Get("http://www.portalfiscal.inf.br/nfe");
                var infProt = nfceProc.Root?.Element(ns + "protNFe")?.Element(ns + "infProt");
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
                _ => "indefinido"
            };
        }
    }
}
