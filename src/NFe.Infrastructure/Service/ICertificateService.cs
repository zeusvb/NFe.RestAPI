using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NFe.Infrastructure.ExternalServices
{
    public interface ICertificateService
    {
        Task<X509Certificate2> LoadCertificateAsync(string certificatePath, string password);
        Task<bool> ValidateCertificateAsync(X509Certificate2 certificate);
    }
}