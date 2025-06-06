﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/authorization")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IAuthorizationService authorizationService; // Fully qualified name to avoid conflicts

        public AuthorizationController(IUserRepository userRepository, IAuthorizationService authorizationService)
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