using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NFe.Domain.DTOs.ZeusDfe;
using NFe.Domain.Interfaces;

namespace NFe.RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ZeusDfeController : ControllerBase
    {
        private readonly IZeusDfeService _zeusDfeService;
        private readonly ILogger<ZeusDfeController> _logger;

        public ZeusDfeController(IZeusDfeService zeusDfeService, ILogger<ZeusDfeController> logger)
        {
            _zeusDfeService = zeusDfeService;
            _logger = logger;
        }

        /// <summary>
        /// Emitir nova NFe via zeus.dfe
        /// </summary>
        [HttpPost("nfe/emitir")]
        [ProducesResponseType(typeof(ZeusDfeEmitirNfeResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> EmitirNfe([FromBody] ZeusDfeEmitirNfeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Emitindo NFe via zeus.dfe para CNPJ: {RecipientCnpj}", request.RecipientCnpj);
                var response = await _zeusDfeService.EmitirNfeAsync(request);

                if (!response.Success)
                    return BadRequest(response);

                return CreatedAtAction(nameof(Consultar), new { accessKey = response.AccessKey }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Dados inválidos: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao emitir NFe");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Emitir nova NFCe via zeus.dfe
        /// </summary>
        [HttpPost("nfce/emitir")]
        [ProducesResponseType(typeof(ZeusDfeEmitirNfeResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> EmitirNfce([FromBody] ZeusDfeEmitirNfeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Emitindo NFCe via zeus.dfe para CNPJ: {RecipientCnpj}", request.RecipientCnpj);
                var response = await _zeusDfeService.EmitirNfceAsync(request);

                if (!response.Success)
                    return BadRequest(response);

                return CreatedAtAction(nameof(Consultar), new { accessKey = response.AccessKey }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Dados inválidos: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao emitir NFCe");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Consultar status de uma NF via zeus.dfe
        /// </summary>
        [HttpPost("consultar")]
        [ProducesResponseType(typeof(ZeusDfeConsultarResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Consultar([FromBody] ZeusDfeConsultarRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Consultando NF via zeus.dfe: {AccessKey}", request.AccessKey);
                var response = await _zeusDfeService.ConsultarAsync(request);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Dados inválidos: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao consultar NF");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Cancelar uma NF autorizada via zeus.dfe
        /// </summary>
        [HttpPost("cancelar")]
        [ProducesResponseType(typeof(ZeusDfeCancelarResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Cancelar([FromBody] ZeusDfeCancelarRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Cancelando NF via zeus.dfe: {AccessKey}", request.AccessKey);
                var response = await _zeusDfeService.CancelarAsync(request);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Dados inválidos: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao cancelar NF");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Verificar conectividade com API zeus.dfe
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                _logger.LogInformation("Verificando health check do zeus.dfe");
                var isHealthy = await _zeusDfeService.HealthCheckAsync();

                if (isHealthy)
                    return Ok(new { status = "healthy", message = "API zeus.dfe está operacional" });

                return StatusCode(503, new { status = "unhealthy", message = "API zeus.dfe não está respondendo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar health check");
                return StatusCode(503, new { status = "error", message = "Erro ao verificar conectividade" });
            }
        }
    }
}