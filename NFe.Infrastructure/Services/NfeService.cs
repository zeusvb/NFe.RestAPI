using Microsoft.EntityFrameworkCore;
using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;
using NFe.Domain.Entities;
using NFe.Infrastructure.Data;
using System.Xml.Linq;

namespace NFe.Infrastructure.Services
{
    public class NfeService : INfeService
    {
        private readonly NfeDbContext _dbContext;
        private readonly ISefazService _sefazService;

        public NfeService(NfeDbContext dbContext, ISefazService sefazService)
        {
            _dbContext = dbContext;
            _sefazService = sefazService;
        }

        public async Task<NfeResponse> EmitirNfeAsync(EmitirNfeRequest request)
        {
            var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId);
            if (company == null)
                throw new ArgumentException("Empresa não encontrada");

            var nextNumber = await GetNextNfeNumberAsync(request.CompanyId, request.Series);
            var xmlContent = BuildNfeXml(request, nextNumber);
            var protocolNumber = await _sefazService.EnviarNfeAsync(xmlContent, string.Empty, string.Empty);
            var accessKey = GenerateAccessKey(company.Cnpj, request.Series, nextNumber);

            var document = new NfeDocument
            {
                CompanyId = request.CompanyId,
                NfeNumber = nextNumber,
                Series = request.Series,
                ProtocolNumber = protocolNumber,
                AccessKey = accessKey,
                Status = "Autorizada",
                XmlContent = xmlContent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.NfeDocuments.Add(document);
            await _dbContext.SaveChangesAsync();

            return new NfeResponse
            {
                Id = document.Id,
                NfeNumber = document.NfeNumber,
                Series = document.Series,
                ProtocolNumber = document.ProtocolNumber,
                Status = document.Status,
                AccessKey = document.AccessKey,
                CreatedAt = document.CreatedAt
            };
        }

        public async Task<ConsultarNfeResponse> ConsultarNfeAsync(string accessKey)
        {
            var document = await _dbContext.NfeDocuments.AsNoTracking()
                .FirstOrDefaultAsync(n => n.AccessKey == accessKey);

            if (document == null)
                throw new ArgumentException("NFe não encontrada");

            return new ConsultarNfeResponse
            {
                ProtocolNumber = document.ProtocolNumber,
                Status = document.Status,
                Message = "Consulta realizada com sucesso",
                StatusDate = document.UpdatedAt
            };
        }

        public async Task<NfeResponse> CancelarNfeAsync(int nfeId, string justificativa)
        {
            var document = await _dbContext.NfeDocuments.FirstOrDefaultAsync(n => n.Id == nfeId);
            if (document == null)
                throw new ArgumentException("NFe não encontrada");

            document.ProtocolNumber = await _sefazService.CancelarNfeAsync(document.AccessKey, justificativa, string.Empty, string.Empty);
            document.Status = "Cancelada";
            document.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return new NfeResponse
            {
                Id = document.Id,
                NfeNumber = document.NfeNumber,
                Series = document.Series,
                ProtocolNumber = document.ProtocolNumber,
                Status = document.Status,
                AccessKey = document.AccessKey,
                CreatedAt = document.CreatedAt
            };
        }

        private async Task<string> GetNextNfeNumberAsync(int companyId, string series)
        {
            var documents = await _dbContext.NfeDocuments.AsNoTracking()
                .Where(d => d.CompanyId == companyId && d.Series == series)
                .OrderByDescending(d => d.Id)
                .Select(d => d.NfeNumber)
                .ToListAsync();

            var lastNumber = documents
                .Select(number => int.TryParse(number, out var parsed) ? parsed : 0)
                .DefaultIfEmpty(0)
                .Max();

            if (lastNumber == 0 && documents.Count > 0)
                lastNumber = documents.Count;

            return (lastNumber + 1).ToString();
        }

        private static string BuildNfeXml(EmitirNfeRequest request, string nfeNumber)
        {
            var xml = new XElement("NFe",
                new XElement("Numero", nfeNumber),
                new XElement("Serie", request.Series),
                new XElement("Emitente", request.CompanyId),
                new XElement("Destinatario",
                    new XElement("Cnpj", request.DestinationCnpj),
                    new XElement("Nome", request.DestinationName)),
                new XElement("Itens",
                    request.Items.Select(item => new XElement("Item",
                        new XElement("Codigo", item.Code),
                        new XElement("Descricao", item.Description),
                        new XElement("Quantidade", item.Quantity),
                        new XElement("ValorUnitario", item.UnitValue))))
            );

            return xml.ToString(SaveOptions.DisableFormatting);
        }

        private static string GenerateAccessKey(string cnpj, string series, string nfeNumber)
        {
            var stateCode = "52";
            var issueDate = DateTime.UtcNow.ToString("yyMM");
            var normalizedCnpj = new string(cnpj.Where(char.IsDigit).ToArray()).PadLeft(14, '0');
            normalizedCnpj = normalizedCnpj[^14..];
            var model = "55";
            var normalizedSeries = (int.TryParse(series, out var parsedSeries) ? parsedSeries : 1).ToString("000");
            var normalizedNumber = (int.TryParse(nfeNumber, out var parsedNumber) ? parsedNumber : 1).ToString("000000000");
            var emissionType = "1";
            var randomCode = Random.Shared.Next(0, 100000000).ToString("00000000");

            var baseKey = $"{stateCode}{issueDate}{normalizedCnpj}{model}{normalizedSeries}{normalizedNumber}{emissionType}{randomCode}";
            var checkDigit = CalculateModulo11(baseKey);
            return $"{baseKey}{checkDigit}";
        }

        private static int CalculateModulo11(string key)
        {
            var multiplier = 2;
            var sum = 0;

            for (var i = key.Length - 1; i >= 0; i--)
            {
                sum += (key[i] - '0') * multiplier;
                multiplier = multiplier == 9 ? 2 : multiplier + 1;
            }

            var remainder = sum % 11;
            return remainder == 0 || remainder == 1 ? 0 : 11 - remainder;
        }
    }
}
