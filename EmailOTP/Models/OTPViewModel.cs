namespace EmailOTP.Models
{
    public class OTPViewModel
    {
        public string? Username { get; set; }
        public string? otp { get; set; }
        public StatusCode Status { get; set; }

        public enum StatusCode
        {
            STATUS_EMAIL_OK,
            STATUS_EMAIL_FAIL,
            STATUS_EMAIL_INVALID,
            STATUS_OTP_OK,
            STATUS_OTP_FAIL,
            STATUS_OTP_TIMEOUT
        }
    }
}