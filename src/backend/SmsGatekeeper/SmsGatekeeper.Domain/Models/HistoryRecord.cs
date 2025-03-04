namespace SmsGatekeeper.Domain.Models
{
    public class HistoryRecord
    {
        public long Id { get; set; }
        public DateTime RequestedAt { get; set; }
        public string PhoneNumber { get; set; }
        public string AccountId { get; set; }
        public bool Allowed { get; set; }

        public bool AccountRateHit { get; set; }
    }
}
