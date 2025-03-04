namespace SmsGatekeeper.Models
{
    public class SmsRateLimitOptions
    {
        public int PerAccount { get; set; }
        public int PerNumber { get; set; }
    }
}
