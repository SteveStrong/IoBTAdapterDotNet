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
    public class AdapterHubController : ControllerBase
    {
        private readonly IHubContext<AdapterHub> adapterHub;

        public AdapterHubController(IHubContext<AdapterHub> adapterHub)
        {
            this.adapterHub = adapterHub;
        }

        [HttpGet("Ping")]
        public async Task SendPing(string payload)
        {
            var data = $"Controller pong message {payload}";
            await this.adapterHub.Clients.All.SendAsync("Pong", data);
        }

        [HttpPost("ContextWrapper")]
        public async Task<ActionResult<ContextWrapper<Success>>> SendContext(object context)
        {
            try
            {
                await this.adapterHub.Clients.All.SendAsync("ReceiveContextPayload", context);
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
                await this.adapterHub.Clients.All.SendAsync("Command", payload);
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
