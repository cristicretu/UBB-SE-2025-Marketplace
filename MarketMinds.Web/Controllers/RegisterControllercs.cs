using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMarketplace.Models;
using SharedClassLibrary.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using SharedClassLibrary.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebMarketplace.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(IUserService userService, ILogger<RegisterController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: /Register
        public IActionResult Index()
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

        // POST: /Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill out all fields correctly.";
                return RedirectToAction("Index", "Register");
            }

            // Check if the email already exists
            var existingUser = await _userService.GetUserByEmail(model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "An account with this email already exists.";
                return RedirectToAction("Index", "Register");
            }

            // Hash the password
            var hashedPassword = _userService.HashPassword(model.Password);

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

                // Automatically log the user in
                var user = await _userService.GetUserByEmail(model.Email);
                UserSession.CurrentUserId = user.UserId;
                TempData["SuccessMessage"] = "Registration successful! Welcome!";
                return RedirectToAction("Index", "Home"); // Redirect to Home after successful login
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while creating your account. Please try again.";
                return RedirectToAction("Index", "Register");
            }
        }
    }
}
