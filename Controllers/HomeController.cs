using IoBTAdapterDotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace IoBTAdapterDotNet.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public ActionResult getDefault()
        {
            return Redirect("/swagger/index.html");
        }

        [HttpGet("swagger")]
        public ActionResult getSwagger()
        {
            return Redirect("/swagger/index.html");
        }

        [HttpGet("api/ping")]
        public ActionResult<ContextWrapper<Success>> GetPing()
        {
            try
            {
                var wrap = new ContextWrapper<Success>(new Success()
                {
                    Message = "up",
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
