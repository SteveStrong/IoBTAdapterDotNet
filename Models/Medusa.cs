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

        ISuccessOrFailure Slew();
    }

    public class MedusaEntity : IMedusaEntity
    {
        public string sourceGuid { get; set; }
        public string timeStamp { get; set; }
        public string personId { get; set; }

        private string ServerAddress = @"http://127.0.0.1:80";

        public async Task<ISuccessOrFailure> HelloWorld() {
            Console.WriteLine("Medusa HelloWorld");
            Console.Beep();

            try {
                using var channel = GrpcChannel.ForAddress("https://localhost:6001");

                var client =  new CompleteGreeter.CompleteGreeterClient(channel);

                var reply = await client.SayHelloAsync(new HelloRequest { Name = "Medusa GreeterClient" });


                var result = new Success() {
                    Message =  $"HelloWorld worked {reply.Message}"
                };
                return result;
                
            } catch(Exception ex) {
                var result = new Failure() {
                    Message =  $"HelloWorld failed {ex.Message}"
                };
                return result;
            }
        }


       public async Task<string> Authenticate(string name)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{ServerAddress}/generateJwtToken?name={name}"),
                Method = HttpMethod.Get,
                Version = new Version(2, 0),
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async ISuccessOrFailure Slew() {
            Console.WriteLine("Medusa Slew");
            Console.Beep();

            //using var channel = GrpcChannel.ForAddress("https://localhost:6001");

            //private static final String CREDENTIALS_NAME_KEY = "ClientName";
            //private static final String CREDENTIALS_TOKEN_KEY = "ClientToken";

            var username = "Squire-Sdk-Client";
            //var password = "Fdhver-Fqx-Pyvrag";

             var token = await Authenticate(username);


//Username: Squire-Sdk-Client
//Password: Fdhver-Fqx-Pyvrag

            // var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
            // var options = new GrpcChannelOptions();
            // options.Credentials = channelCredentials;
            // //options.Credentials = 

            // //ChannelCredentials.SetUserNameCredential "username", "password"

            // var channel = GrpcChannel.ForAddress(ServerAddress, new GrpcChannelOptions
            // {
            //     Credentials = ChannelCredentials.Create(new SslCredentials(), CallCredentials.FromInterceptor((context, metadata) =>
            //     {
            //         metadata.Add("Authorization", $"Bearer {token}");
            //         return Task.CompletedTask;
            //     }))
            // })


            using var channel = GrpcChannel.ForAddress("http://127.0.0.1:52523", options);

            // "Only 'http' and 'https' schemes are allowed. (Parameter 'requestUri')"
            // using var channel = GrpcChannel.ForAddress("tcp://127.0.0.1:52523");
            var client =  new DevicesManager.DevicesManagerClient(channel);

            var arg = new Google.Protobuf.WellKnownTypes.Empty();
            var reply = client.getVersion(arg);


            var result = new Success() {
                Message =  $"Slew worked {reply.Version}"
            };
            return result;
        }

    }


}
