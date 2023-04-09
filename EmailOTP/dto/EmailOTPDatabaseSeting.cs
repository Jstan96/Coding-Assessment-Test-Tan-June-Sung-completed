namespace EmailOTP.dto
{
    public class EmailOTPDatabaseSeting
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string EmailCollectionName { get; set; } = null!;
    }
}
