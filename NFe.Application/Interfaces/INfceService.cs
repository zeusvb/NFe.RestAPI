using System.Threading.Tasks;
using NFe.Application.DTOs.NFe;

namespace NFe.Application.Interfaces
{
    public interface INfceService
    {
        Task<NfeResponse> EmitirNfceAsync(EmitirNfeRequest request);
        Task<ConsultarNfeResponse> ConsultarNfceAsync(string accessKey);
    }
}