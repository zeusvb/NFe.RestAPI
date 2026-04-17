using Microsoft.EntityFrameworkCore;
using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;
using NFe.Domain.Entities;
using NFe.Infrastructure.Data;

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

            var xmlContent = "<nfe />";
            var protocolNumber = await _sefazService.EnviarNfeAsync(xmlContent, string.Empty, string.Empty);
            var accessKey = Guid.NewGuid().ToString("N");

            var document = new NfeDocument
            {
                CompanyId = request.CompanyId,
                NfeNumber = DateTime.UtcNow.Ticks.ToString(),
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
    }
}
