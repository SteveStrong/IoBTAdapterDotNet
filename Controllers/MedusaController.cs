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
        private readonly IMedusaEntity medusaEntity;

        public MedusaController(IHubContext<MedusaHub> medusaHub, IMedusaEntity medusaEntity)
        {
            this.medusaHub = medusaHub;
            this.medusaEntity = medusaEntity;
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
                if ( payload.command == null) {
                    var error = new ContextWrapper<UDTO_Command>("Command is null");
                    return BadRequest(error);
                }

                //simplify for general topic based commands
                await this.medusaHub.Clients.All.SendAsync(payload.command, payload);
                
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
        public async Task<ActionResult<ContextWrapper<UDTO_Command>>> Slew(UDTO_Command payload)
        {
            try
            {
                //broadcast a command
                if ( payload.command != "SLEW") {
                    var error = new ContextWrapper<UDTO_Command>(payload);
                    return BadRequest(error);
                }
 
                //broadcast a command to medusa
                //var result = await this.medusaEntity.Slew();

                await this.medusaHub.Clients.All.SendAsync("Slew", payload);
                var wrap = new ContextWrapper<UDTO_Command>(payload);
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
