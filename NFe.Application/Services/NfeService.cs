using System.Threading.Tasks;
using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;

namespace NFe.Application.Services
{
    public class NfeService : NFe.Application.Interfaces.INfeService
    {
        public Task<NfeResponse> EmitirNfeAsync(EmitirNfeRequest request)
        {
            // TODO: Implementar emissão de NFe
            throw new System.NotImplementedException("NFe service not implemented yet");
        }

        public Task<ConsultarNfeResponse> ConsultarNfeAsync(string accessKey)
        {
            // TODO: Implementar consulta de NFe
            throw new System.NotImplementedException("NFe service not implemented yet");
        }

        public Task<NfeResponse> CancelarNfeAsync(int nfeId, string justificativa)
        {
            // TODO: Implementar cancelamento de NFe
            throw new System.NotImplementedException("NFe service not implemented yet");
        }
    }
}