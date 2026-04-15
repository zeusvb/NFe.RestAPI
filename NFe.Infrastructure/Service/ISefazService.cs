using System.Threading.Tasks;

namespace NFe.Infrastructure.ExternalServices
{
    public interface ISefazService
    {
        Task<string> EnviarNfeAsync(string xmlContent, string certificatePath, string certificatePassword);
        Task<string> ConsultarStatusAsync(string accessKey, string certificatePath, string certificatePassword);
        Task<string> CancelarNfeAsync(string accessKey, string justificativa, string certificatePath, string certificatePassword);
    }
}