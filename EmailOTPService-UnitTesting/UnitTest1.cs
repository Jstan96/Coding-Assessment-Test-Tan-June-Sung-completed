using EmailOTP.Services;
using System.Net.NetworkInformation;

namespace EmailOTPService_UnitTesting
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("test@test.dso.org.sg", true)]  //valid email
        [InlineData("test@test.com", false)]        //invalid domain
        [InlineData("12321421", false)]             //invalid email format
        public async void ValidateEmail(string userEmail, bool expected)
        {
            // Arrange

            //Act
            EmailOTPService service = new EmailOTPService();
            var status = service.IsValidEmail(userEmail);


            // Assert
            Assert.Equal(expected, status);
            
        }

        [Theory]
        [InlineData("xiA/4Jg/phISL/eINxcYieSueShZ9nr80hqjpQnyNgM=", "xiA/4Jg/phISL/eINxcYieSueShZ9nr80hqjpQnyNgM=", 1,  true)]  //valid
        [InlineData("xiA/4Jg/phISL/eINxcYieSueShZ9nr80hqjpQnyNgM=", "xiA/4Jg/phISL/eINxcYieSueShZ9nr80hqjpQnyNgM=", 2, false)]  //expiry
        [InlineData("xiA/4Jg/phISL/eINxcYieSueShZ9nr80hqjpQnyNgM=", "xasdsa", 1, false)]    //invalid otp
        [InlineData("sdasdsad", "xiA/4Jg/phISL/eINxcYieSueShZ9nr80hqjpQnyNgM=", 2, false)]   //invalid otp
        public async void ValidateOtp(string otp, string dbOtp, int expirydate, bool expected)
        {
            // Arrange
            DateTime Expirydatetime = DateTime.UtcNow;
            switch (expirydate)
            {
                case 1:
                    Expirydatetime = DateTime.UtcNow.AddMinutes(1);
                    break;
                case 2:
                    Expirydatetime = DateTime.UtcNow.AddMinutes(-1);
                    break;

            }
            //Act
            EmailOTPService service = new EmailOTPService();
            var status = service.IsValidotpHash(otp, dbOtp, Expirydatetime);


            // Assert
            Assert.Equal(expected, status);

        }
    }
}