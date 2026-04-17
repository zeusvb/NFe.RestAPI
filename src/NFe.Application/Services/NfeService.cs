using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;

namespace NFe.Application.Services
{
    public class NfeService : INfeService
    {
        public Task<NfeResponse> EmitirNfeAsync(EmitirNfeRequest request)
        {
            return Task.FromResult(new NfeResponse
            {
                Id = 0,
                NfeNumber = DateTime.UtcNow.Ticks.ToString(),
                Series = request.Series,
                ProtocolNumber = string.Empty,
                Status = "Pendente",
                AccessKey = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            });
        }

        public Task<ConsultarNfeResponse> ConsultarNfeAsync(string accessKey)
        {
            return Task.FromResult(new ConsultarNfeResponse
            {
                ProtocolNumber = string.Empty,
                Status = "Em processamento",
                Message = $"Consulta registrada para chave {accessKey}",
                StatusDate = DateTime.UtcNow
            });
        }

        public Task<NfeResponse> CancelarNfeAsync(int nfeId, string justificativa)
        {
            return Task.FromResult(new NfeResponse
            {
                Id = nfeId,
                NfeNumber = string.Empty,
                Series = string.Empty,
                ProtocolNumber = string.Empty,
                Status = "Cancelada",
                AccessKey = string.Empty,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
