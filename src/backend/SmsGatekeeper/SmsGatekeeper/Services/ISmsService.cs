using SmsGatekeeper.Models;

namespace SmsGatekeeper.Services
{
    public interface ISmsService
    {
        public ValueTask<bool> CanSendSms(string accountId, PhoneNumber phoneNumber);
    }
}
