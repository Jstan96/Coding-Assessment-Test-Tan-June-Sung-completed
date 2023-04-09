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
                    return RedirectToAction("VerifyOTP", "Email", new { OTPViewModel.Username });
                case OTPViewModel.StatusCode.STATUS_EMAIL_INVALID:
                    return NotFound($"{user.email} not found");
                case OTPViewModel.StatusCode.STATUS_EMAIL_FAIL:
                    return NotFound($"{user.email} not found");
                default:
                    return NotFound();
            }
        }

        public IActionResult VerifyOTP(string username)
        {
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
                    return RedirectToAction("VerifyOTP", "Email", new { username });
                case OTPViewModel.StatusCode.STATUS_EMAIL_FAIL:
                    return RedirectToAction("VerifyOTP", "Email", new { username });
                default:
                    return RedirectToAction();
            }

        }

        public IActionResult SuccessView()
        {
            return View();
        }
    }
}
