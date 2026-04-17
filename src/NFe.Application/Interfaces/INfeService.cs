using System.Threading.Tasks;
using NFe.Application.DTOs.NFe;

namespace NFe.Application.Interfaces
{
    public interface INfeService
    {
        Task<NfeResponse> EmitirNfeAsync(EmitirNfeRequest request);
        Task<ConsultarNfeResponse> ConsultarNfeAsync(string accessKey);
        Task<NfeResponse> CancelarNfeAsync(int nfeId, string justificativa);
    }
}