
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Serilog.Events;
using SmsGatekeeper.Models;
using SmsGatekeeper.Services;
using StackExchange.Redis;

namespace SmsGatekeeper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Information()
                 .WriteTo.Console()

                 .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSerilog();


            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

            builder.Services.AddScoped<ISmsService, SmsService>();

            builder.Services.Configure<SmsRateLimitOptions>(builder.Configuration.GetSection("SmsRateLimitOptions"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();
            Log.Information("SmsGatekeeper Version: {appVersion} is running", GetVersion());
            app.Run();
        }

        public static string GetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }
    }
}
