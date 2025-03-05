using Serilog;
using SmsDataApi.HostedServices;
using SmsDataApi.Hubs;
using StackExchange.Redis;

namespace SmsDataApi
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
            builder.Services.AddCors();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSerilog();
            builder.Services.AddHostedService<RealtimeUpdatesService>();
            builder.Services.AddSignalR();
            

            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(b =>
            {
                b.SetIsOriginAllowed(_ => true) // In development, allow any origin
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials();
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapHub<SmsHub>("/metrics"); // Changed to match frontend URL

            app.Run();
        }
    }
}
