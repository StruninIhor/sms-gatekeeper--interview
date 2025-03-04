using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmsGatekeeper.Models
{
    public readonly struct PhoneNumber
    {
        public string Value { get; }

        public PhoneNumber(string phoneNumber) 
        {
            if (!IsValidPhoneNumber(phoneNumber))
                throw new ArgumentException("Invalid phone number format.");

            Value = phoneNumber;
        }

        public static bool IsValidPhoneNumber(string phoneNumber) =>
            phoneNumber?.Length == 11 && phoneNumber.All(char.IsDigit);

        public override string ToString() => Value;

        public class PhoneNumberConverter : JsonConverter<PhoneNumber>
        {
            public override PhoneNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
           => new(reader.GetString() ?? throw new Exception("Empty phone number!"));

            public override void Write(Utf8JsonWriter writer, PhoneNumber value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.Value);
        }

    }
}
