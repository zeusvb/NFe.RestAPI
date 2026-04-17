using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NFe.Infrastructure.ExternalServices
{
    public class CertificateService : ICertificateService
    {
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(ILogger<CertificateService> logger)
        {
            _logger = logger;
        }

        public async Task<X509Certificate2> LoadCertificateAsync(string certificatePath, string password)
        {
            try
            {
                _logger.LogInformation("Carregando certificado de: {CertificatePath}", certificatePath);

                if (!File.Exists(certificatePath))
                    throw new FileNotFoundException($"Certificado não encontrado: {certificatePath}");

                var certificateData = await File.ReadAllBytesAsync(certificatePath);
                var certificate = new X509Certificate2(certificateData, password);

                if (!await ValidateCertificateAsync(certificate))
                    throw new InvalidOperationException("Certificado inválido ou expirado");

                _logger.LogInformation("Certificado carregado com sucesso");
                return certificate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar certificado");
                throw;
            }
        }

        public Task<bool> ValidateCertificateAsync(X509Certificate2 certificate)
        {
            try
            {
                if (certificate == null)
                    return Task.FromResult(false);

                var now = DateTime.Now;
                if (now < certificate.NotBefore || now > certificate.NotAfter)
                {
                    _logger.LogWarning("Certificado expirado ou ainda não válido");
                    return Task.FromResult(false);
                }

                _logger.LogInformation("Certificado válido até: {ExpiryDate}", certificate.NotAfter);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar certificado");
                return Task.FromResult(false);
            }
        }
    }
}