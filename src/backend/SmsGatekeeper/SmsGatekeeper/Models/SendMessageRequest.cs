using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmsGatekeeper.Models
{
    public class SendMessageRequest
    {
        [Required]
        [JsonConverter(typeof(PhoneNumber.PhoneNumberConverter))]
        public PhoneNumber  PhoneNumber { get; set; }

        [Required]
        public string AccountId { get; set; }
    }
}
