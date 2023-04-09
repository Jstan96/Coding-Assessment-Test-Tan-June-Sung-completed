using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace EmailOTP.Models
{
    public  class EmailOTPModules
    {
        public const int STATUS_EMAIL_OK = 0;
        public const int STATUS_EMAIL_FAIL = 1;
        public const int STATUS_EMAIL_INVALID = 2;

        protected string smtpServer;
        protected int smtpPort;
        protected string smtpUsername;
        protected string smtpPassword;
        protected string senderEmail;

        public EmailOTPModules(IConfiguration _configuration)
        {
            this.smtpServer = _configuration.GetValue<string>("AppSettings:smtpServer");
            this.smtpPort = _configuration.GetValue<int>("AppSettings:smtpPort"); 
            this.smtpUsername = _configuration.GetValue<string>("AppSettings:smtpUsername");
            this.smtpPassword = _configuration.GetValue<string>("AppSettings:smtpPassword");
            this.senderEmail = _configuration.GetValue<string>("AppSettings:senderEmail");
        }

        public int generate_OTP_email(string user_email)
        {
            // Check if user email is valid
            if (IsValidEmail(user_email))
            {
                return STATUS_EMAIL_INVALID;
            }

            // Generate a 6-digit OTP
            var otp = GenerateOtp();

             // Send OTP email
             var smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            smtpClient.EnableSsl = true;

            var message = new MailMessage(senderEmail, user_email);
            message.Subject = "Your OTP Code";
            message.Body = $"Your OTP Code is {otp}. The code is valid for 1 minute.";

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception)
            {
                return STATUS_EMAIL_FAIL;
            }

            return STATUS_EMAIL_OK;
        }

        public bool check_OTP(Func<int> readOTP, int generated_otp)
        {
            const int NUM_TRIES = 10;
            const int OTP_LENGTH = 6;
            const int TIMEOUT_SECONDS = 60;

            for (int i = 0; i < NUM_TRIES; i++)
            {
                // Read OTP from input
                int otp = 0;
                try
                {
                    otp = readOTP();
                }
                catch
                {
                    continue; // Skip if input is not an integer
                }

                if (otp.ToString().Length != OTP_LENGTH)
                {
                    continue; // Skip if input is not 6 digits
                }

                // Check if OTP is valid
                if (otp == generated_otp)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsValidEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email) && email.EndsWith(".dso.org.sg");
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

        private static string otphash(int otp)
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
