namespace EmailOTP.dto
{
    public class EmailSettings
    {

        public string smtpServer { get; set; } = null!;

        public int smtpPort { get; set; }
        public string smtpUsername { get; set; } = null!;
        public string smtpPassword { get; set; } = null!;
        public string senderEmail { get; set; } = null!;

    }
}
