using Microsoft.AspNetCore.Mvc;
using System;

namespace NFe.RestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Health check da API
        /// </summary>
        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}