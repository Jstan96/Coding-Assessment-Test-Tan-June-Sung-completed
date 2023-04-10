Email OTP Module
This is an email OTP module for .NET Core MVC applications. It allows users to verify their email address using a one-time password (OTP) sent to their email.

Features
Sends OTP to user's email address
Verifies OTP entered by user
Uses session to pass data between controllers
Installation
To use this module in your .NET Core MVC application, follow these steps:
1. Install-Package MongoDB.Driver
2. Whitelist currentIP inorder to use mongodb database

Usage
To use the email OTP module in your .NET Core MVC application, follow these steps:
Navigate to the Email controller's Index action to enter your email address.
The system will send an OTP to the entered email address or display it over the page.
after successfully sent the email, it will redirect to VerifyOTP action to enter the OTP sent to your email.
The system will verify the entered OTP and redirect you to the success or error view.


Assumptions:
1. The input stream provided to the check_OTP() function implements a readOTP() function that returns the 6-digit OTP entered by the user.
2. The user has a valid and active email account with the ".dso.org.sg" domain.
3. The email service used to send OTP emails is secure and complies with data protection regulations.

Testing the module:
1. Unit tests: Write unit tests to test the generate_OTP_email() and check_OTP() functions. 
The generate_OTP_email function should be tested with a variety of input email addresses, including invalid addresses, valid addresses from non-"dso.org.sg" domains, and valid addresses from "dso.org.sg" domains.
The check_OTP function should be tested with a variety of input streams, including streams that do not provide an OTP within the 1 minute timeout, and streams that provide invalid OTPs
2. Integration tests: Test the module's integration with the rest of the application, including input validation and error handling.
3. Load testing: Test the module's performance and scalability by simulating a large number of requests to generate and check OTPs simultaneously.
4. Security testing: Test the module's security measures, such as checking for email domain restrictions and ensuring secure email transmission.
5. Usability testing: Test the user experience of the module, including ease of use and clear instructions for the OTP generation and checking process.
