using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;

namespace NFe.Application.Services
{
    public class NfceService : INfceService
    {
        public Task<NfeResponse> EmitirNfceAsync(EmitirNfeRequest request)
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

        public Task<ConsultarNfeResponse> ConsultarNfceAsync(string accessKey)
        {
            return Task.FromResult(new ConsultarNfeResponse
            {
                ProtocolNumber = string.Empty,
                Status = "Em processamento",
                Message = $"Consulta registrada para chave {accessKey}",
                StatusDate = DateTime.UtcNow
            });
        }
    }
}
