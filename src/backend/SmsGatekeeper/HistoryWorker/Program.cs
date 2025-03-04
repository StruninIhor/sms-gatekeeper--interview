using Microsoft.EntityFrameworkCore;
using Serilog;
using SmsGatekeeper.Domain;
using StackExchange.Redis;
using System.Diagnostics;

namespace HistoryWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Information()
              .WriteTo.Console()
              .CreateLogger();

            Log.Information("Starting History Worker");

            try
            {
                var builder = Host.CreateApplicationBuilder(args);
                builder.Services.AddHostedService<Worker>();
                builder.Services.AddSerilog();

                builder.Services.AddSingleton<IConnectionMultiplexer>(
                    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

                builder.Services.AddDbContext<SmsContext>(o =>
                {
                    o.UseSqlServer(builder.Configuration.GetConnectionString("SmsDatabase"));
                });

                var host = builder.Build();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal error while running application");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }
    }
}