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

        [HttpPost("emitir")]
        [ProducesResponseType(typeof(NfeResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> EmitirNfce([FromBody] EmitirNfeRequest request)
        {
            try
            {
                var response = await _nfceService.EmitirNfceAsync(request);
                return CreatedAtAction(nameof(ConsultarNfce), new { accessKey = response.AccessKey }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Dados inválidos para NFC-e: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao emitir NFC-e");
                return StatusCode(500, new { message = "Erro ao emitir NFC-e" });
            }
        }

        [HttpGet("consultar/{accessKey}")]
        [ProducesResponseType(typeof(ConsultarNfeResponse), 200)]
        public async Task<IActionResult> ConsultarNfce(string accessKey)
        {
            var response = await _nfceService.ConsultarNfceAsync(accessKey);
            return Ok(response);
        }
    }
}
