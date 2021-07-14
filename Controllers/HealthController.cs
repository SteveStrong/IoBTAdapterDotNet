using IoBTAdapterDotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace IoBTAdapterDotNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {

        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }


        // [HttpOptions]
        // public IActionResult PreflightRoute()
        // {
        //     return NoContent();
        // }

        [HttpGet]
        public ActionResult<ContextWrapper<Success>> GetHealth()
        {
            try
            {
                var wrap = new ContextWrapper<Success>(new Success() { 
                    Status = true
                });

                return Ok(wrap);
            }
            catch (Exception ex)
            {
                var wrap = new ContextWrapper<Success>(ex.Message);
                return BadRequest(wrap);
            }
        }
    }
}
