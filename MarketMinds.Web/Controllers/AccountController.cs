using System.Security.Claims;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.UserService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketMinds.Web.Models;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Services;
using WebMarketplace.Models;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, IAccountRepository accountRepository, ILogger<AccountController> logger)
        {
            _userService = userService;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            var captcha = GenerateCaptcha();
            ViewData["CaptchaCode"] = captcha;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Please fill all fields correctly.");
                ViewData["CaptchaCode"] = GenerateCaptcha();
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            if (!VerifyCaptcha(model.CaptchaInput))
            {
                ModelState.AddModelError(string.Empty, "Captcha verification failed.");
                ViewData["CaptchaCode"] = GenerateCaptcha();
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            if (!_userService.IsEmailValidForLogin(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Invalid email format.");
                ViewData["CaptchaCode"] = GenerateCaptcha();
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
            
            var user = await _userService.GetUserByEmail(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                ViewData["CaptchaCode"] = GenerateCaptcha();
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            if (await _userService.IsUserSuspended(model.Email))
            {
                ModelState.AddModelError(string.Empty, "User is suspended.");
                ViewData["CaptchaCode"] = GenerateCaptcha();
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // Use PasswordHash for comparison instead of Password
            if (user.PasswordHash != _userService.HashPassword(model.Password))
            {
                await _userService.HandleFailedLogin(model.Email);
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                ViewData["CaptchaCode"] = GenerateCaptcha();
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            await this._userService.AuthorizationLogin();
            await _userService.ResetFailedLogins(model.Email);
            
            // Set UserSession for backward compatibility
            UserSession.CurrentUserId = user.Id;
            UserSession.CurrentUserRole = (int)user.UserType == 2 ? "Buyer" : "Seller";

            await SignInUserAsync(user);
            
            _logger.LogInformation($"User {user.Username} (ID: {user.Id}) logged in successfully with role {UserSession.CurrentUserRole}");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            // Creates a list of roles
            var roles = new List<SelectListItem>
            {
                new SelectListItem { Text = "Buyer", Value = "Buyer" },
                new SelectListItem { Text = "Seller", Value = "Seller" }
            };

            // Passes the list of roles to the view
            ViewBag.Roles = roles;
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill out all fields correctly.";
                var roles = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Buyer", Value = "Buyer" },
                    new SelectListItem { Text = "Seller", Value = "Seller" }
                };
                ViewBag.Roles = roles;
                return View(model);
            }

            // Check if the email already exists
            var existingUser = await _userService.GetUserByEmail(model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "An account with this email already exists.";
                var roles = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Buyer", Value = "Buyer" },
                    new SelectListItem { Text = "Seller", Value = "Seller" }
                };
                ViewBag.Roles = roles;
                return View(model);
            }

            var role = 0; // Default role   
            //Transforms the role string to an integer
            if (model.Role == "Buyer")
            { role = 2; }
            else if (model.Role == "Seller")
            { role = 3; }

            // Save the user in the database
            _logger.LogInformation($"Registering user: {model.Username}, Email: {model.Email}, Role: {model.Role}");    
            var createdUser = await _userService.RegisterUser(model.Username, model.Password, model.Email, model.Telephone, role);

            // Check if the user was successfully created
            if (createdUser)
            {
                await this._userService.AuthorizationLogin();

                // Get the newly created user
                var user = await _userService.GetUserByEmail(model.Email);
                if (user != null)
                {
                    // Set UserSession for backward compatibility
                    UserSession.CurrentUserId = user.Id;
                    UserSession.CurrentUserRole = model.Role;
                    
                    // Set up authentication cookie
                    await SignInUserAsync(user);
                    
                    TempData["SuccessMessage"] = "Registration successful! Welcome!";
                    return RedirectToAction("Index", "Home");
                }
            }
            
            TempData["ErrorMessage"] = "An error occurred while creating your account. Please try again.";
            var rolesList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Buyer", Value = "Buyer" },
                new SelectListItem { Text = "Seller", Value = "Seller" }
            };
            ViewBag.Roles = rolesList;
            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clear UserSession for backward compatibility
            UserSession.CurrentUserId = null;
            UserSession.CurrentUserRole = null;
            
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Account/Index
        public async Task<IActionResult> Index()
        {
            // Get current user ID from claims
            int userId = User.Identity.IsAuthenticated 
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) 
                : UserSession.CurrentUserId ?? 0;
            
            // If not authenticated, redirect to login
            if (userId == 0)
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Fetch user data
                var user = await _accountRepository.GetUserByIdAsync(userId);
                
                // Fetch orders 
                var orders = await _accountRepository.GetUserOrdersAsync(userId);
                
                // Create view model with null checks
                var model = new AccountViewModel
                {
                    User = user != null ? new MarketMinds.Shared.Models.UserDto(user) : new MarketMinds.Shared.Models.UserDto 
                    {
                        Id = userId,
                        Username = User.Identity?.Name ?? "Unknown",
                        Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                        Balance = 0
                    },
                    Orders = orders ?? new List<MarketMinds.Shared.Models.UserOrder>()
                };
                
                return View("~/Views/Home/Account.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account data for user {UserId}", userId);
                
                // Create fallback model with data from claims
                var fallbackModel = new AccountViewModel
                {
                    User = new MarketMinds.Shared.Models.UserDto
                    {
                        Id = userId,
                        Username = User.Identity?.Name ?? "Unknown",
                        Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                        Balance = 0
                    },
                    Orders = new List<MarketMinds.Shared.Models.UserOrder>()
                };
                
                return View("~/Views/Home/Account.cshtml", fallbackModel);
            }
        }

        // Helper methods for captcha
        private string GenerateCaptcha()
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

        // Helper method to sign in a user
        private async Task SignInUserAsync(User user)
        {
            // Make sure we have the username
            if (string.IsNullOrEmpty(user.Username))
            {
                _logger.LogWarning($"Username is empty for user ID {user.Id}. Using email as username.");
                user.Username = user.Email;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };
            
            // Add role claim based on user type
            string role = (int)user.UserType == 2 ? "Buyer" : "Seller";
            claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                IssuedUtc = DateTimeOffset.UtcNow,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
            _logger.LogInformation($"Authentication cookie created for user {user.Username}");
        }
    }
} 