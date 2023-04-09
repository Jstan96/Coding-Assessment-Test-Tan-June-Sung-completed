using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace EmailOTP.Models
{
    public class Email
    {

        public ObjectId Id { get; set; }
        public string? email { get; set; }

        public string? OtpHash { get; set; }

        public DateTime OTPExpiry { get; set; }
    }
}
