using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;

namespace NFe.Infrastructure.Services
{
    public class NfceService : INfceService
    {
        private readonly INfeService _nfeService;

        public NfceService(INfeService nfeService)
        {
            _nfeService = nfeService;
        }

        public Task<NfeResponse> EmitirNfceAsync(EmitirNfeRequest request)
        {
            return _nfeService.EmitirNfeAsync(request);
        }

        public Task<ConsultarNfeResponse> ConsultarNfceAsync(string accessKey)
        {
            return _nfeService.ConsultarNfeAsync(accessKey);
        }
    }
}
