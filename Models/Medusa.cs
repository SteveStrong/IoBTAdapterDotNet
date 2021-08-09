using System;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;

using MedusaAdapter;


namespace IoBTAdapterDotNet.Models
{
     
// public static final Metadata.Key<String> METADATA_NAME_KEY = Metadata.Key.of(CREDENTIALS_NAME_KEY, ASCII_STRING_MARSHALLER);
// public static final Metadata.Key<String> METADATA_TOKEN_KEY = Metadata.Key.of(CREDENTIALS_TOKEN_KEY, ASCII_STRING_MARSHALLER);

// public class ClientCredentials(String clientName, String clientToken)
// {
//   this.clientName = clientName;
//   this.clientToken = clientToken;

//   headers = new Metadata();

//   headers.put(METADATA_NAME_KEY, clientName);
//   headers.put(METADATA_TOKEN_KEY, clientToken);
// }
 
// @Override
// public void applyRequestMetadata(RequestInfo requestInfo, Executor appExecutor, MetadataApplier applier)
// {
//   appExecutor.execute(() -> applier.apply(headers));
// }

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

        private string ServerAddress = @"https://127.0.0.1:80";

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

            var response = client.Send(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<ISuccessOrFailure> Slew() {
            Console.WriteLine("Medusa Slew");
            Console.Beep();

			string DEVICES_MGR_CONNECTION = "http://127.0.0.1:52523"; //:52530";
			string CLIENT_NAME_KEY = "ClientName";
			string CLIENT_NAME = "Squire-Sdk-Client";
			string CLIENT_TOKEN_KEY = "ClientToken";
			string CLIENT_TOKEN = "Fdhver-Fqx-Pyvrag";

			var headers = new Grpc.Core.Metadata {
				new Metadata.Entry(CLIENT_NAME_KEY, CLIENT_NAME),
				new Metadata.Entry(CLIENT_TOKEN_KEY, CLIENT_TOKEN)
			};

            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            using var channel = GrpcChannel.ForAddress(DEVICES_MGR_CONNECTION);

            // "Only 'http' and 'https' schemes are allowed. (Parameter 'requestUri')"
            // using var channel = GrpcChannel.ForAddress("tcp://127.0.0.1:52523");
            var client =  new DevicesManager.DevicesManagerClient(channel);

			// this call gets all the device configuration definitions in the system.
			// you cannot inject call credentials into the channel for an insecure connection,
			// which means you have to pass the headers into each call instead.

            // I do not think this works because I do not know if the server is sending us devices
            var xx = new Empty();
			var devicesStream = client.getDevices(xx, headers);
			await foreach ( var device in devicesStream.ResponseStream.ReadAllAsync() ) {
				Console.WriteLine(device);
			}

            // this code worked and we got a an APLHA version number
            var version = await client.getVersionAsync(xx, headers);


            var result = new Success() {
                Message =  $"Slew version  {version}"
            };
            return result;
        }

    }


}
