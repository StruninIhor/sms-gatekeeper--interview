using Microsoft.EntityFrameworkCore;
using SmsGatekeeper.Domain;
using SmsGatekeeper.Domain.Models;
using StackExchange.Redis;

namespace HistoryWorker
{
    public class Worker(ILogger<Worker> logger, IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IConnectionMultiplexer _redis = redis;
        private readonly string _channelName = "rate_limit_events";
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber();

            await subscriber.SubscribeAsync(RedisChannel.Pattern(_channelName), async (channel, message) =>
            {
                try
                {
                    var parts = message.ToString().Split(':');
                    if (parts.Length < 5 || parts.Length > 6)
                    {
                        _logger.LogCritical("Invalid data received: {message}", message);
                        return;
                    }

                    var status = parts[1];
                    var isAllowed = status == "allowed";

                    int idIndex = isAllowed ? 2 : 3;

                    var timestamp = parts[idIndex + 2];
                    var timestampInfo = timestamp.Split('.');
                    var (unixTimeStamp, microseconds) = (long.Parse(timestampInfo[0]), timestampInfo.Length > 1 ? int.Parse(timestampInfo[1]) : 0);
                    DateTime requestedAt = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).UtcDateTime + TimeSpan.FromMicroseconds(microseconds);


                    var record = new HistoryRecord
                    {
                        Allowed = status == "allowed",
                        AccountRateHit = status == "blocked" && parts[2] == "account",
                        AccountId = parts[idIndex],
                        PhoneNumber = parts[idIndex + 1],
                        RequestedAt = requestedAt
                    };

                    _logger.LogInformation("New record: {@record}", record);

                    await using var scope = _scopeFactory.CreateAsyncScope();
                    await using var context = scope.ServiceProvider.GetService<SmsContext>();
                    context.HistoryRecords.Add(record);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Exception while saving data, message is {message}", message);
                }
            });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
