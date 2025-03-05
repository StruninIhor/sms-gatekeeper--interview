using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR;
using SmsDataApi.Hubs;

namespace SmsDataApi.HostedServices
{
    public class RealtimeUpdatesService(ILogger<RealtimeUpdatesService> logger, IConnectionMultiplexer redis, IServiceProvider serviceProvider) : BackgroundService
    {
        private ILogger<RealtimeUpdatesService> _logger = logger;
        private readonly IConnectionMultiplexer _redis = redis;
        private readonly string _channelName = "rate_limit_events";
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        private int _eventCount = 0;
        private bool _sentZero = false;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(RealtimeUpdatesService)} starting");
            var subscriber = _redis.GetSubscriber();

            await subscriber.SubscribeAsync(RedisChannel.Pattern(_channelName), (channel, message) =>
            {
                //potential bottleneck, redis key might be better
                Interlocked.Increment(ref _eventCount);
            });
            _logger.LogInformation($"{nameof(RealtimeUpdatesService)} subscribed to redis pub/sub");


            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

                var count = Interlocked.Exchange(ref _eventCount, 0);
                if (count > 0 || !_sentZero)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SmsHub>>();
                        _logger.LogInformation($"Dispatching {count} events");
                        await hubContext.Clients.All.SendAsync("MetricsUpdate", new { count }, stoppingToken);
                    }
                    _sentZero = count == 0;
                }
            }
        }
    }
}

