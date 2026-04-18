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
    public class NfceController : ControllerBase
    {
        private readonly INfceService _nfceService;
        private readonly ILogger<NfceController> _logger;

        public NfceController(INfceService nfceService, ILogger<NfceController> logger)
        {
            _nfceService = nfceService;
            _logger = logger;
        }

        /// <summary>
        /// Emitir nova NFCe (Nota Fiscal do Consumidor Eletrônica)
        /// </summary>
        [HttpPost("emitir")]
        [ProducesResponseType(typeof(NfeResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> EmitirNfce([FromBody] EmitirNfeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Emitindo NFCe para empresa: {CompanyId}", request.CompanyId);
                var response = await _nfceService.EmitirNfceAsync(request);

                if (!response.Success)
                    return BadRequest(response);

                return CreatedAtAction(nameof(ConsultarNfce), new { accessKey = response.AccessKey }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Dados inválidos: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao emitir NFCe");
                return StatusCode(500, new { message = "Erro ao emitir NFCe", error = ex.Message });
            }
        }

        /// <summary>
        /// Consultar status de uma NFCe
        /// </summary>
        [HttpGet("consultar/{accessKey}")]
        [ProducesResponseType(typeof(ConsultarNfeResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ConsultarNfce(string accessKey)
        {
            try
            {
                _logger.LogInformation("Consultando NFCe: {AccessKey}", accessKey);
                var response = await _nfceService.ConsultarNfceAsync(accessKey);

                if (!response.Success)
                    return NotFound(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar NFCe");
                return StatusCode(500, new { message = "Erro ao consultar NFCe", error = ex.Message });
            }
        }

        /// <summary>
        /// Obter informações de emissão de NFCe (documentação)
        /// </summary>
        [HttpGet("info")]
        [AllowAnonymous]
        public IActionResult GetInfo()
        {
            return Ok(new
            {
                title = "API de Emissão de NFCe",
                version = "1.0",
                description = "Serviço de emissão de Notas Fiscais do Consumidor Eletrônicas integrado com zeus.dfe",
                endpoints = new
                {
                    emitir = "/api/nfce/emitir",
                    consultar = "/api/nfce/consultar/{accessKey}"
                },
                requirements = new
                {
                    authentication = "Bearer Token (JWT)",
                    cnpj = "CNPJ do emitente registrado no banco de dados",
                    itens = "Mínimo 1 item com código, descrição, quantidade, preço unitário"
                }
            });
        }
    }
}
