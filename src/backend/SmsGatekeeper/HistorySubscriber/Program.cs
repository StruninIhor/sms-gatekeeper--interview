using Serilog;
using StackExchange.Redis;

namespace HistorySubscriber
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // var context 
            var mux = ConnectionMultiplexer.Connect("localhost");
            var subscriber = mux.GetSubscriber();

            subscriber.Subscribe(RedisChannel.Pattern("rate_limit_events"), (channel, type) =>
            {
                var key = GetKey(channel);
            });

            Log.Information("History Subscriber is running!");

            Console.ReadLine();
        }

        static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Information()
              .WriteTo.Console()
              .CreateLogger();
        }

        static string GetKey(string channel)
        {
            var index = channel.IndexOf(':');

            if (index >= 0 && index < channel.Length - 1)
            {
                return channel[(index + 1)..];
            }

            return channel;
        }
    }
}
