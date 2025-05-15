using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMarketplace.Models;
using SharedClassLibrary.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using SharedClassLibrary.Domain;

namespace WebMarketplace.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IUserService userService, ILogger<LoginController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Generates a random captcha and stores it in session
        public string GenerateCaptcha()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var captchaCode = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
            HttpContext.Session.SetString("CaptchaCode", captchaCode);
            return captchaCode;
        }

        private bool VerifyCaptcha(string enteredCaptcha)
        {
            var generatedCaptcha = HttpContext.Session.GetString("CaptchaCode");
            return string.Equals(enteredCaptcha, generatedCaptcha, StringComparison.OrdinalIgnoreCase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all fields correctly.";
                return RedirectToAction("Index");
            }

            if (!VerifyCaptcha(model.CaptchaInput))
            {
                TempData["ErrorMessage"] = "Captcha verification failed.";
                return RedirectToAction("Index");
            }

            if (!_userService.IsEmailValidForLogin(model.Email))
            {
                TempData["ErrorMessage"] = "Invalid email format.";
                return RedirectToAction("Index");
            }
            
            var user = await _userService.GetUserByEmail(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            if (await _userService.IsUserSuspended(model.Email))
            {
                TempData["ErrorMessage"] = "User is suspended.";
                return RedirectToAction("Index");
            }

            if (user.Password != _userService.HashPassword(model.Password))
            {
                await _userService.HandleFailedLogin(model.Email);
                TempData["ErrorMessage"] = "Invalid credentials.";
                return RedirectToAction("Index");
            }

            await this._userService.AuthorizationLogin();

            await _userService.ResetFailedLogins(model.Email);
            UserSession.CurrentUserId = user.UserId;
            if((int)user.Role==2)
            {
                UserSession.CurrentUserRole = "Buyer";
                return RedirectToAction("Index", "BuyerProfile");
            }
            UserSession.CurrentUserRole = "Seller";

            return RedirectToAction("Index", "SellerProfile");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var captcha = GenerateCaptcha();
            ViewData["CaptchaCode"] = captcha;
            return View();
        }
    }
}
