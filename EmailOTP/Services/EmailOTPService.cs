using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using System.Text;
using EmailOTP.Models;
using Microsoft.Extensions.Options;
using EmailOTP.dto;
using System.Security.Cryptography;
using MongoDB.Driver;
using static System.Net.WebRequestMethods;
using static EmailOTP.Models.OTPViewModel;
using Microsoft.AspNetCore.ResponseCaching;

namespace EmailOTP.Services
{
    public class EmailOTPService
    {
        private readonly IMongoCollection<Email> _emailsCollection;
        private readonly Random _random = new Random();

        public const int STATUS_EMAIL_OK = 0;
        public const int STATUS_EMAIL_FAIL = 1;
        public const int STATUS_EMAIL_INVALID = 2;

        protected string smtpServer;
        protected int smtpPort;
        protected string smtpUsername;
        protected string smtpPassword;
        protected string senderEmail;

        public EmailOTPService() { }
        public EmailOTPService(
            IOptions<EmailSettings> EmailSettings,
            IOptions<EmailOTPDatabaseSeting> EmailOTPDatabaseSettings)
        {
            var mongoClient = new MongoClient(
               EmailOTPDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                EmailOTPDatabaseSettings.Value.DatabaseName);

            _emailsCollection = mongoDatabase.GetCollection<Email>(
                EmailOTPDatabaseSettings.Value.EmailCollectionName);

            this.smtpServer = EmailSettings.Value.smtpServer;
            this.smtpPort = EmailSettings.Value.smtpPort;
            this.smtpUsername = EmailSettings.Value.smtpUsername;
            this.smtpPassword = EmailSettings.Value.smtpPassword;
            this.senderEmail = EmailSettings.Value.senderEmail;
        }

        public async Task<OTPViewModel> GenerateOTP(string userEmail)
        {
            OTPViewModel response = new OTPViewModel();

            // Check if email is valid and from allowed domain
            if (!IsValidEmail(userEmail))
            {
                response.Status = StatusCode.STATUS_EMAIL_INVALID;
                return response;
            }

            // Generate a new 6 digit OTP code
            string otp = GenerateOtp();

            var hashtop = otphash(otp);
            // Save the OTP code and expiry time in the database
            var filter = Builders<Email>.Filter.Eq("email", userEmail);
            var update = Builders<Email>.Update
                .Set("OtpHash", hashtop)
                .Set("OTPExpiry", DateTime.UtcNow.AddMinutes(1));
            var result = await _emailsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0 && result.MatchedCount == 0)
            {
                // No document was found with the specified email address, so insert a new one
                var email = new Email
                {
                    email = userEmail,
                    OtpHash = hashtop,
                    OTPExpiry = DateTime.UtcNow.AddMinutes(1)
                };
                await _emailsCollection.InsertOneAsync(email);
            }

            // Send the OTP code to the user's email address
            string emailBody = $"Your OTP code is {otp}. The code is valid for 1 minute.";
            bool emailSent = SendEmail(userEmail, emailBody);

            if (emailSent)
            {
                response.Username = userEmail;
                response.otpMessage = emailBody;
                response.Status = StatusCode.STATUS_EMAIL_OK;
                return response;
            }

            response.Status = StatusCode.STATUS_EMAIL_FAIL;
            return response;
        }

        public async Task<StatusCode> CheckOTP(string userEmail, string otp)
        {
            // Check if OTP code exists and is valid
            var filter = Builders<Email>.Filter.Eq("email", userEmail);
            var user = _emailsCollection.Find(filter).FirstOrDefault();
            if (otp.ToString().Length != 6)
            {
                return StatusCode.STATUS_OTP_FAIL;
            }

            var otpHash = otphash(otp.ToString());

            if (user != null && IsValidotpHash(otpHash, user.OtpHash,user.OTPExpiry))
            {
                // Clear the OTP code and expiry time from the database
                var update = Builders<Email>.Update
                    .Unset("OtpHash")
                    .Unset("OTPExpiry");
                _emailsCollection.UpdateOne(filter, update);

                return StatusCode.STATUS_OTP_OK;
            }
            else if (user != null && user.OTPExpiry <= DateTime.UtcNow)
            {
                return StatusCode.STATUS_OTP_TIMEOUT;
            }

            return StatusCode.STATUS_OTP_FAIL;
        }


        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.EndsWith(".dso.org.sg");
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidotpHash(string otp, string DBotp, DateTime Otpexpiry)
        {
            return (otp == DBotp && Otpexpiry > DateTime.UtcNow) ? true : false;

        }

        private bool SendEmail(string user_email, string otp)
        {
            // Implement your own email sending logic here
            // Send OTP email
            var smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            smtpClient.EnableSsl = true;

            var message = new MailMessage(this.senderEmail, user_email);
            message.Subject = "Your OTP Code";
            message.Body = $"Your OTP Code is {otp}. The code is valid for 1 minute.";

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private static string GenerateOtp()
        {
            const int otpLength = 6;
            var random = new Random();
            var otp = new StringBuilder();

            for (var i = 0; i < otpLength; i++)
            {
                otp.Append(random.Next(0, 9));
            }

            return otp.ToString();
        }

        private static string otphash(string otp)
        {
            string salt = "otphash";

            // Combine the password and salt value
            string combined = otp + salt;

            // Create a SHA256 hash object
            SHA256 sha256 = SHA256.Create();

            // Compute the hash value of the combined string
            byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));

            // Convert the hash value to a string and store it in the database
            string hashedPassword = Convert.ToBase64String(hashValue);

            return hashedPassword;
        }
    }
}
