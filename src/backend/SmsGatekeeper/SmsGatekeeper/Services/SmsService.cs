using Microsoft.Extensions.Options;
using SmsGatekeeper.Models;
using StackExchange.Redis;

namespace SmsGatekeeper.Services
{
    public class SmsService(ILogger<SmsService> logger, IConnectionMultiplexer mux, IOptions<SmsRateLimitOptions> options) : ISmsService
    {
        private readonly IDatabase _db = mux.GetDatabase();
        private readonly ILogger<SmsService> _logger = logger;
        private readonly IOptions<SmsRateLimitOptions> _options = options;

        public async ValueTask<bool> CanSendSms(string accountId, PhoneNumber phoneNumber)
        {
            var allowed = ((int)await _db.ScriptEvaluateAsync(Scripts.SlidingRateLimiterScript,
                new
                {

                    account_key = new RedisKey($"sms_rate:account:{accountId}"),
                    phone_key = new RedisKey($"sms_rate:number:{phoneNumber}"),
                    account_id = accountId,
                    phone = $"{phoneNumber}",
                    window = 1000, // 1 second 
                    per_number = _options.Value.PerNumber,
                    per_account = _options.Value.PerAccount,
                    expire_time = 2 // 2 seconds
                })) == 1;

            return allowed;
        }


        static class Scripts
        {
            public static LuaScript SlidingRateLimiterScript => LuaScript.Prepare(SlidingRateLimiter);

            /* Modified sliding window rate limiter script
             * https://redis.io/learn/develop/dotnet/aspnetcore/rate-limiting/sliding-window
             */
            private const string SlidingRateLimiter = @"
                local current_time = redis.call('TIME')
                local now = tonumber(current_time[1]) * 1000 + tonumber(current_time[2]) / 1000

                local trim_time = tonumber(current_time[1]) - @window
                local event_channel = 'rate_limit_events'
  
                redis.call('ZREMRANGEBYSCORE', @phone_key, 0, trim_time)
                redis.call('ZREMRANGEBYSCORE', @account_key, 0, trim_time)

                local account_count = redis.call('ZCARD', @account_key)

                local event_message = string.format('%s:%s:%s', @account_id, @phone, now)
        
            
                if account_count >= tonumber(@per_account) then
                    redis.call('PUBLISH', event_channel, 'rate_limit:blocked:account:' .. event_message)
                    return 0
                end

                redis.call('ZADD', @account_key, current_time[1], current_time[1] .. current_time[2])
                redis.call('EXPIRE', @account_key, @expire_time)

                local phone_count = redis.call('ZCARD', @phone_key)
        

                if phone_count >= tonumber(@per_number) then
                    redis.call('ZREM', @account_key, now)
                    redis.call('PUBLISH', event_channel, 'rate_limit:blocked:phone:' .. event_message)
                    return 0
                end
                redis.call('ZADD', @phone_key, current_time[1], current_time[1] .. current_time[2])
                redis.call('EXPIRE', @phone_key, @expire_time)
                redis.call('PUBLISH', event_channel, 'rate_limit:allowed:' .. event_message)
                return 1
            ";

            
        }

    }
}