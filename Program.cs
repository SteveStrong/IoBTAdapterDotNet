using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Runtime.InteropServices;

namespace IoBTAdapterDotNet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => 
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                     webBuilder.ConfigureKestrel(options =>
                     {
                         // Setup a HTTP/2 endpoint without TLS.
                         options.ListenLocalhost(5000, o => o.Protocols = 
                             HttpProtocols.Http2);
                     });
                    }
                    webBuilder.UseStartup<Startup>();
                });
        
        


        // public static IHostBuilder CreateHostBuilderOriginal(string[] args) =>
        //     Host.CreateDefaultBuilder(args)
        //         .ConfigureWebHostDefaults(webBuilder =>
        //         {
        //             webBuilder.UseStartup<Startup>();
        //         });
    }
}
