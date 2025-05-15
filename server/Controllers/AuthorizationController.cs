using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Service;
using System.Net;
using SharedClassLibrary.Helper;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/authorization")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly SharedClassLibrary.Service.IAuthorizationService authorizationService; // Fully qualified name to avoid conflicts

        public AuthorizationController(IUserRepository userRepository, SharedClassLibrary.Service.IAuthorizationService authorizationService)
        {
            this.userRepository = userRepository;
            this.authorizationService = authorizationService;
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            // Generate JWT token
            var token = this.authorizationService.GenerateJwtToken();

            // Set JWT in secure, HttpOnly cookie
            this.Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1),
            });

            AppConfig.AuthorizationToken = token;

            Debug.WriteLine("JwtToken created: " + token);

            return this.Ok(token);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            this.Response.Cookies.Delete("access_token");

            return this.Ok(new { message = "Logged out successfully" });
        }
    }
}