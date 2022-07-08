using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.IO;

namespace TCPWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(progData, "PinstoreTCPListener", "servicelog.txt"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)                
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
