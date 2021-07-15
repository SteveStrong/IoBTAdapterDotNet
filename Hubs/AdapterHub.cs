using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using System.Diagnostics;
using IoBTAdapterDotNet.Models;

// https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-5.0&tabs=visual-studio

//dotnet dev-certs https --clean
//dotnet dev-certs https --trust


// https://www.youtube.com/watch?v=Lws0zOaseIM

namespace IoBTAdapterDotNet.Hubs
{
    public interface IAdapterHub
    {
        Task<string> Ping(string payload);

        Task<UDTO_Command> Command(UDTO_Command payload);
    }
    public class AdapterHub : Hub, IAdapterHub
    {


        public AdapterHub()
        {
        }


        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }


        public async Task<string> Ping(string payload)
        {
            await Clients.All.SendAsync("Pong", payload);
            return payload;
        }





        public async Task<UDTO_Command> Command(UDTO_Command payload)
        {
            var msg = "Command";

            await Clients.All.SendAsync(msg, payload);
            return payload;
        }
    }
}