using System.Threading.Tasks;
using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;

namespace NFe.Application.Services
{
    public class NfceService : INfceService
    {
        public Task<NfeResponse> EmitirNfceAsync(EmitirNfeRequest request)
        {
            // TODO: Implementar emissão de NFCe
            throw new System.NotImplementedException("NFCe service not implemented yet");
        }

        public Task<ConsultarNfeResponse> ConsultarNfceAsync(string accessKey)
        {
            // TODO: Implementar consulta de NFCe
            throw new System.NotImplementedException("NFCe service not implemented yet");
        }
    }
}
