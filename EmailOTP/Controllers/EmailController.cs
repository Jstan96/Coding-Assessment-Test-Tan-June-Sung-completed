using EmailOTP.Models;
using EmailOTP.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EmailOTP.Controllers
{
    public class EmailController : Controller
    {
        private readonly EmailOTPService _emailsService;
        public EmailController(EmailOTPService emailsService)
        {
            _emailsService = emailsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Email user)
        {
            var OTPViewModel =  await _emailsService.GenerateOTP(user.email);
            switch (OTPViewModel.Status)
            {
                case OTPViewModel.StatusCode.STATUS_EMAIL_OK:
                    
                    HttpContext.Session.SetString("OtpMessage", OTPViewModel.otpMessage);
                    HttpContext.Session.SetString("username", OTPViewModel.Username);
                    return RedirectToAction("VerifyOTP", "Email");
                case OTPViewModel.StatusCode.STATUS_EMAIL_INVALID:
                    TempData["ErrorMessage"] = $"{user.email} not found";
                    return RedirectToAction("index", "Email");
                case OTPViewModel.StatusCode.STATUS_EMAIL_FAIL:
                    TempData["ErrorMessage"] = $"{user.email} not found";
                    return RedirectToAction("index", "Email");
                default:
                    return RedirectToAction("index", "Email");
            }
        }

        public IActionResult VerifyOTP()
        {
            string username = HttpContext.Session.GetString("username");
            if (username == null)
            {
                TempData["ErrorMessage"] = $"{username} not found";
                    return RedirectToAction("index", "Email");
            }

            var model = new OTPViewModel { Username = username };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string username, string otp)
        {
            var status = await _emailsService.CheckOTP(username, otp);
            switch (status)
            {
                case OTPViewModel.StatusCode.STATUS_OTP_OK:
                    return RedirectToAction("SuccessView", "Email");
                case OTPViewModel.StatusCode.STATUS_OTP_FAIL:
                    TempData["ErrorMessage"] = $"{username} otp verify failed.please try again";
                    return RedirectToAction("VerifyOTP", "Email");
                case OTPViewModel.StatusCode.STATUS_EMAIL_FAIL:
                    TempData["ErrorMessage"] = $"{username} not found";
                    return RedirectToAction("VerifyOTP", "Email"); 
                case OTPViewModel.StatusCode.STATUS_OTP_TIMEOUT:
                    TempData["ErrorMessage"] = $"{username} OTP timeout. please try again";
                    return RedirectToAction("index", "Email");
                default:
                    TempData["ErrorMessage"] = " Unknown Request";
                    return RedirectToAction("index", "Email", null);
            }

        }

        public IActionResult SuccessView()
        {
            return View();
        }
    }
}
