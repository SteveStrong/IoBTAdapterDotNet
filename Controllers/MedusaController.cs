using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;

using IoBTAdapterDotNet.Models;
using IoBTAdapterDotNet.Hubs;

namespace IoBTAdapterDotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedusaController : ControllerBase
    {
        private readonly IHubContext<MedusaHub> medusaHub;

        public MedusaController(IHubContext<MedusaHub> medusaHub)
        {
            this.medusaHub = medusaHub;
        }

        [HttpGet("Ping")]
        public async Task SendPing(string payload)
        {
            var data = $"Controller pong message {payload}";
            await this.medusaHub.Clients.All.SendAsync("Pong", data);
        }

        [HttpPost("ContextWrapper")]
        public async Task<ActionResult<ContextWrapper<Success>>> SendContext(object context)
        {
            try
            {
                await this.medusaHub.Clients.All.SendAsync("ReceiveContextPayload", context);
                var wrap = new ContextWrapper<Success>(new Success()
                {
                    Status = true
                });
                return Ok(wrap);
            }
            catch (Exception ex)
            {
                var wrap = new ContextWrapper<Success>(ex.Message);
                return BadRequest(ex);
            }
        }


        [HttpPost("Command")]
        public async Task<ActionResult<ContextWrapper<UDTO_Command>>> Command(UDTO_Command payload)
        {
            try
            {
                await this.medusaHub.Clients.All.SendAsync("Command", payload);
                var wrap = new ContextWrapper<UDTO_Command>(payload);
                return Ok(wrap);
            }
            catch (Exception ex)
            {
                var wrap = new ContextWrapper<UDTO_Command>(ex.Message);
                return BadRequest(wrap);
            }
        }


        [HttpPost("Slew")]
        public async Task<ActionResult<ContextWrapper<UDTO_Command>>> Slew()
        {
            try
            {
                var slew = new UDTO_Command();

                await this.medusaHub.Clients.All.SendAsync("Command", slew);
                var wrap = new ContextWrapper<UDTO_Command>(slew);
                return Ok(wrap);
            }
            catch (Exception ex)
            {
                var wrap = new ContextWrapper<UDTO_Command>(ex.Message);
                return BadRequest(wrap);
            }
        }

    }
}
