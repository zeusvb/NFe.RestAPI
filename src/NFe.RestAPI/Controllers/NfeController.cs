using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NFe.Application.DTOs.NFe;
using NFe.Application.Interfaces;

namespace NFe.RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NfeController : ControllerBase
    {
        private readonly INfeService _nfeService;
        private readonly ILogger<NfeController> _logger;

        public NfeController(INfeService nfeService, ILogger<NfeController> logger)
        {
            _nfeService = nfeService;
            _logger = logger;
        }

        /// <summary>
        /// Emitir nova NFe
        /// </summary>
        [HttpPost("emitir")]
        [ProducesResponseType(typeof(NfeResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> EmitirNfe([FromBody] EmitirNfeRequest request)
        {
            try
            {
                _logger.LogInformation("Emitindo NFe para empresa: {CompanyId}", request.CompanyId);
                var response = await _nfeService.EmitirNfeAsync(request);
                return CreatedAtAction(nameof(ConsultarNfe), new { accessKey = response.AccessKey }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Dados inválidos: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao emitir NFe");
                return StatusCode(500, new { message = "Erro ao emitir NFe" });
            }
        }

        /// <summary>
        /// Consultar status de uma NFe
        /// </summary>
        [HttpGet("consultar/{accessKey}")]
        [ProducesResponseType(typeof(ConsultarNfeResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ConsultarNfe(string accessKey)
        {
            try
            {
                _logger.LogInformation("Consultando NFe: {AccessKey}", accessKey);
                var response = await _nfeService.ConsultarNfeAsync(accessKey);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar NFe");
                return StatusCode(500, new { message = "Erro ao consultar NFe" });
            }
        }

        /// <summary>
        /// Cancelar NFe
        /// </summary>
        [HttpPost("{nfeId}/cancelar")]
        [ProducesResponseType(typeof(NfeResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelarNfe(int nfeId, [FromQuery] string justificativa)
        {
            try
            {
                _logger.LogInformation("Cancelando NFe: {NfeId}", nfeId);
                var response = await _nfeService.CancelarNfeAsync(nfeId, justificativa);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar NFe");
                return StatusCode(500, new { message = "Erro ao cancelar NFe" });
            }
        }
    }
}