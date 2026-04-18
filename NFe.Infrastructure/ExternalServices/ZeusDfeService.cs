using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NFe.Domain.DTOs.ZeusDfe;
using NFe.Domain.Interfaces;

namespace NFe.Infrastructure.ExternalServices
{
    /// <summary>
    /// Implementação do cliente HTTP para zeus.dfe
    /// </summary>
    public class ZeusDfeService : IZeusDfeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZeusDfeService> _logger;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public ZeusDfeService(HttpClient httpClient, IConfiguration configuration, ILogger<ZeusDfeService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _baseUrl = _configuration["ZeusDfe:BaseUrl"] ?? throw new InvalidOperationException("ZeusDfe:BaseUrl não configurado");
            _apiKey = _configuration["ZeusDfe:ApiKey"] ?? throw new InvalidOperationException("ZeusDfe:ApiKey não configurado");

            ConfigurarHttpClient();
        }

        private void ConfigurarHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<ZeusDfeEmitirNfeResponse> EmitirNfeAsync(ZeusDfeEmitirNfeRequest request)
        {
            return await EmitirDocumentoAsync(request, "nfe");
        }

        public async Task<ZeusDfeEmitirNfeResponse> EmitirNfceAsync(ZeusDfeEmitirNfeRequest request)
        {
            return await EmitirDocumentoAsync(request, "nfce");
        }

        private async Task<ZeusDfeEmitirNfeResponse> EmitirDocumentoAsync(ZeusDfeEmitirNfeRequest request, string tipo)
        {
            try
            {
                _logger.LogInformation($"Iniciando emissão de {tipo.ToUpper()} para CNPJ: {request.RecipientCnpj}");

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/v1/{tipo}/emit", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao emitir {tipo}: {response.StatusCode} - {errorContent}");

                    return new ZeusDfeEmitirNfeResponse
                    {
                        Success = false,
                        Message = $"Erro ao comunicar com zeus.dfe: {response.StatusCode}",
                        Status = "erro"
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ZeusDfeEmitirNfeResponse>(responseContent, options);

                if (result.Success)
                {
                    _logger.LogInformation($"Emissão de {tipo} bem-sucedida. Chave: {result.AccessKey}");
                }
                else
                {
                    _logger.LogWarning($"Emissão de {tipo} rejeitada. Mensagem: {result.Message}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exceção ao emitir {tipo}");

                return new ZeusDfeEmitirNfeResponse
                {
                    Success = false,
                    Message = $"Erro: {ex.Message}",
                    Status = "erro"
                };
            }
        }

        public async Task<ZeusDfeConsultarResponse> ConsultarAsync(ZeusDfeConsultarRequest request)
        {
            try
            {
                _logger.LogInformation($"Consultando status da NF: {request.AccessKey}");

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/v1/nf/consult", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao consultar NF: {response.StatusCode} - {errorContent}");

                    return new ZeusDfeConsultarResponse
                    {
                        Success = false,
                        Message = $"Erro ao comunicar com zeus.dfe: {response.StatusCode}"
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ZeusDfeConsultarResponse>(responseContent, options);

                _logger.LogInformation($"Consulta realizada. Status: {result.Status}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção ao consultar NF");

                return new ZeusDfeConsultarResponse
                {
                    Success = false,
                    Message = $"Erro: {ex.Message}"
                };
            }
        }

        public async Task<ZeusDfeCancelarResponse> CancelarAsync(ZeusDfeCancelarRequest request)
        {
            try
            {
                _logger.LogInformation($"Cancelando NF: {request.AccessKey}");

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/v1/nf/cancel", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao cancelar NF: {response.StatusCode} - {errorContent}");

                    return new ZeusDfeCancelarResponse
                    {
                        Success = false,
                        Message = $"Erro ao comunicar com zeus.dfe: {response.StatusCode}"
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ZeusDfeCancelarResponse>(responseContent, options);

                if (result.Success)
                {
                    _logger.LogInformation($"NF cancelada com sucesso. Protocolo: {result.ProtocolNumber}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção ao cancelar NF");

                return new ZeusDfeCancelarResponse
                {
                    Success = false,
                    Message = $"Erro: {ex.Message}"
                };
            }
        }

        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                _logger.LogInformation("Verificando conectividade com zeus.dfe");

                var response = await _httpClient.GetAsync("/api/v1/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar saúde de zeus.dfe");
                return false;
            }
        }
    }
}
