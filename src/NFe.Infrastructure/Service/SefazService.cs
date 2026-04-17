using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NFe.Infrastructure.ExternalServices;

namespace NFe.Infrastructure.ExternalServices
{
    public class SefazService : ISefazService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SefazService> _logger;
        private readonly IAsyncPolicy<string> _retryPolicy;

        public SefazService(IConfiguration configuration, ILogger<SefazService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _retryPolicy = CreateRetryPolicy();
        }

        public async Task<string> EnviarNfeAsync(string xmlContent, string certificatePath, string certificatePassword)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Iniciando envio de NFe para SEFAZ Goiás...");

                try
                {
                    // Aqui você usaria a biblioteca Zeus.Net.NFe.NFCe para enviar
                    // Este é um exemplo simplificado

                    var protocolo = await SendToSefazAsync(xmlContent, certificatePath, certificatePassword);
                    _logger.LogInformation("NFe enviada com sucesso. Protocolo: {Protocolo}", protocolo);
                    return protocolo;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar NFe para SEFAZ");
                    throw;
                }
            });
        }

        public async Task<string> ConsultarStatusAsync(string accessKey, string certificatePath, string certificatePassword)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Consultando status da NFe: {AccessKey}", accessKey);

                try
                {
                    var status = await QuerySefazStatusAsync(accessKey, certificatePath, certificatePassword);
                    return status;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao consultar status");
                    throw;
                }
            });
        }

        public async Task<string> CancelarNfeAsync(string accessKey, string justificativa, string certificatePath, string certificatePassword)
        {
            _logger.LogInformation("Iniciando cancelamento da NFe: {AccessKey}", accessKey);

            try
            {
                var protocolo = await SendCancellationToSefazAsync(accessKey, justificativa, certificatePath, certificatePassword);
                _logger.LogInformation("NFe cancelada com sucesso. Protocolo: {Protocolo}", protocolo);
                return protocolo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar NFe");
                throw;
            }
        }

        private IAsyncPolicy<string> CreateRetryPolicy()
        {
            var retryConfig = _configuration.GetSection("Sefaz:RetryPolicy");
            var maxRetries = int.Parse(retryConfig["MaxRetries"] ?? "3");
            var delayMs = int.Parse(retryConfig["DelayMilliseconds"] ?? "1000");

            return Policy<string>
                .Handle<Exception>()
                .OrResult(r => string.IsNullOrEmpty(r))
                .WaitAndRetryAsync(
                    retryCount: maxRetries,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(delayMs * Math.Pow(2, attempt - 1)),
                    onRetry: (outcome, timespan, attempt, context) =>
                    {
                        _logger.LogWarning("Tentativa {Attempt} de envio. Aguardando {Delay}ms...", attempt, timespan.TotalMilliseconds);
                    }
                );
        }

        private async Task<string> SendToSefazAsync(string xmlContent, string certificatePath, string certificatePassword)
        {
            // Implementação com Zeus.Net.NFe.NFCe
            await Task.Delay(100); // Simular latência
            return Guid.NewGuid().ToString(); // Simular protocolo
        }

        private async Task<string> QuerySefazStatusAsync(string accessKey, string certificatePath, string certificatePassword)
        {
            await Task.Delay(100);
            return "100"; // Simular status (100 = Autorizado)
        }

        private async Task<string> SendCancellationToSefazAsync(string accessKey, string justificativa, string certificatePath, string certificatePassword)
        {
            await Task.Delay(100);
            return Guid.NewGuid().ToString();
        }
    }
}