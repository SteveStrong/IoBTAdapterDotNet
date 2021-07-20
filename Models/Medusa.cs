using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System.Net.Http;
using Grpc.Net.Client;

using MedusaAdapter;


namespace IoBTAdapterDotNet.Models
{
    
    public interface IMedusaEntity
    {
        Task<ISuccessOrFailure> HelloWorld();

        Task<ISuccessOrFailure> Slew();
    }

    public class MedusaEntity : IMedusaEntity
    {
        public string sourceGuid { get; set; }
        public string timeStamp { get; set; }
        public string personId { get; set; }

        public async Task<ISuccessOrFailure> HelloWorld() {
            Console.WriteLine("Medusa HelloWorld");
            Console.Beep();

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var client =  new CompleteGreeter.CompleteGreeterClient(channel);

            var reply = await client.SayHelloAsync(new HelloRequest { Name = "Medusa GreeterClient" });


            var result = new Success() {
                Message =  $"HelloWorld worked {reply.Message}"
            };
            return result;
        }


        public async Task<ISuccessOrFailure> Slew() {
            Console.WriteLine("Medusa Slew");
            Console.Beep();

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var client =  new DevicesManager.DevicesManagerClient(channel);

            var reply = await client.getVersion(new Google.Protobuf.WellKnownTypes.Empty()));


            var result = new Success() {
                Message =  $"Slew worked {reply.Message}"
            };
            return result;
        }

    }


}
