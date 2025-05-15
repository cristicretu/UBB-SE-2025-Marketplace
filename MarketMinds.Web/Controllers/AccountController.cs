using System.Security.Claims;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.UserService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Web.Models;
using MarketMinds.Shared.IRepository;

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
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            try
            {
                var user = await _userService.GetUserByCredentialsAsync(username, password);
                
                if (user != null && user.Id != 0)
                {
                    await SignInUserAsync(user);
                    _logger.LogInformation($"User {username} logged in successfully");
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for user {username}");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        // GET: Account/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isUsernameTaken = await _userService.IsUsernameTakenAsync(user.Username);
                    if (isUsernameTaken)
                    {
                        ModelState.AddModelError("Username", "This username is already taken.");
                        return View(user);
                    }

                    var registeredUser = await _userService.RegisterUserAsync(user);
                    if (registeredUser != null && registeredUser.Id != 0)
                    {
                        await SignInUserAsync(registeredUser);
                        return RedirectToAction("Index", "Home");
                    }
                    
                    ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                }
            }
            
            return View(user);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
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
            int userId = User.GetCurrentUserId();
            
            // If not authenticated, redirect to login
            if (!User.Identity.IsAuthenticated)
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

        // Helper method to sign in a user
        private async Task SignInUserAsync(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
} 